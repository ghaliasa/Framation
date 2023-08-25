namespace Framation
{
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;
    using System.IO;
    using System.Linq;
    using System.Diagnostics;
    using System.Collections;
    using System.Collections.Generic;
    using OpenCvSharp;
    using mattatz.Triangulation2DSystem;
    using mattatz.Triangulation2DSystem.Example;


    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Collider2D))]  // REQUIRES A COLLIDER2D to function
    // 1. Attach this to a read/write enabled sprite image
    // 2. Set the drawing_layers  to use in the raycast
    // 3. Attach a 2D collider (like a Box Collider 2D) to this sprite
    // 4. Hold down left mouse to draw on this texture!
    public class Drawable : MonoBehaviour
    {

        [SerializeField] GameObject prefab;
        Camera cam;
        
        // PEN COLOUR
        public Color Pen_Colour = Color.black;     // Change these to change the default drawing settings
        // PEN WIDTH (actually, it's a radius, in pixels)
        public static int Pen_Width = 5;

        public delegate void Brush_Function(Vector2 world_position);
        // This is the function called when a left click happens
        // Pass in your own custom one to change the brush type
        // Set the default function in the Awake method
        public Brush_Function current_brush;

        public LayerMask Drawing_Layers;

        public bool Reset_Canvas_On_Play = true;
        // The colour the canvas is reset to each time
        public Color Reset_Colour = new Color(1, 1, 1, 1);  // By default, reset the canvas to be transparent

        // Used to reference THIS specific file without making all methods static
        public static Drawable drawable;
        // MUST HAVE READ/WRITE enabled set in the file editor of Unity
        public Sprite drawable_sprite;
        public Texture2D drawable_texture;

        Vector2 previous_drag_position;
        Color[] clean_colours_array;
        Color transparent;
        Color32[] cur_colors;
        bool mouse_was_previously_held_down = false;
        bool no_drawing_on_current_drag = false;

        /// Attributes 
            public List<Vector2> points = new List<Vector2>() ;
            public List<Triangle> triangles ;
            public List<Vector2> uv ;
            public Dictionary<LineController, List<Triangle>> output ;
            public Material materialToModify ;
            public Mesh globalMesh;
            public Link link ;
            public bool moveMesh = false;
            public bool isDrawing = true ;
            public bool DrawTriangulation = false ;
            public bool changeTexture = false ;
            public GameObject go ;


//////////////////////////////////////////////////////////////////////////////
// BRUSH TYPES. Implement your own here

        // When you want to make your own type of brush effects,
        // Copy, paste and rename this function.
        // Go through each step
        public void BrushTemplate(Vector2 world_position)
        {
            // 1. Change world position to pixel coordinates
            Vector2 pixel_pos = WorldToPixelCoordinates(world_position);

            // 2. Make sure our variable for pixel array is updated in this frame
            cur_colors = drawable_texture.GetPixels32();

            ////////////////////////////////////////////////////////////////
            // FILL IN CODE BELOW HERE

            // Do we care about the user left clicking and dragging?
            // If you don't, simply set the below if statement to be:
            //if (true)

            // If you do care about dragging, use the below if/else structure
            if (previous_drag_position == Vector2.zero)
            {
                // THIS IS THE FIRST CLICK
                // FILL IN WHATEVER YOU WANT TO DO HERE
                // Maybe mark multiple pixels to colour?
                MarkPixelsToColour(pixel_pos, Pen_Width, Pen_Colour);
            }
            else
            {
                // THE USER IS DRAGGING
                // Should we do stuff between the previous mouse position and the current one?
                ColourBetween(previous_drag_position, pixel_pos, Pen_Width, Pen_Colour);
            }
            ////////////////////////////////////////////////////////////////

            // 3. Actually apply the changes we marked earlier
            // Done here to be more efficient
            ApplyMarkedPixelChanges();
            
            // 4. If dragging, update where we were previously
            previous_drag_position = pixel_pos;
        }

        // Default brush type. Has width and colour.
        // Pass in a point in WORLD coordinates
        // Changes the surrounding pixels of the world_point to the static pen_colour
        public void PenBrush(Vector2 world_point)
        {
            Vector2 pixel_pos = WorldToPixelCoordinates(world_point);

            cur_colors = drawable_texture.GetPixels32();

            if (previous_drag_position == Vector2.zero)
            {
                // If this is the first time we've ever dragged on this image, simply colour the pixels at our mouse position
                MarkPixelsToColour(pixel_pos, Pen_Width, Pen_Colour);
            }
            else
            {
                // Colour in a line from where we were on the last update call
                ColourBetween(previous_drag_position, pixel_pos, Pen_Width, Pen_Colour);
            }
            ApplyMarkedPixelChanges();

            //Debug.Log("Dimensions: " + pixelWidth + "," + pixelHeight + ". Units to pixels: " + unitsToPixels + ". Pixel pos: " + pixel_pos);
            previous_drag_position = pixel_pos;
        }

        // Helper method used by UI to set what brush the user wants
        // Create a new one for any new brushes you implement
        public void SetPenBrush()
        {
            // PenBrush is the NAME of the method we want to set as our current brush
            current_brush = PenBrush;
        }
//////////////////////////////////////////////////////////////////////////////

        // This is where the magic happens.
        // Detects when user is left clicking, which then call the appropriate function
        void Update()
        {

            if(Drawing.drawSkeltonMode){
                isDrawing = !isDrawing;

                // BEGIN Set the texture on texture_material
                    
                    Texture2D texture = new Texture2D((int)drawable_sprite.rect.width, (int)drawable_sprite.rect.height);
                    texture.SetPixels(
                        drawable_sprite.texture.GetPixels(
                            (int)drawable_sprite.textureRect.x,
                            (int)drawable_sprite.textureRect.y,
                            (int)drawable_sprite.textureRect.width,
                            (int)drawable_sprite.textureRect.height
                        )
                    );
                    
                    texture.Apply();

                    materialToModify.SetTexture("_MainTex", texture);
                    
                // End Set the texture on texture_material
                
                ConvertSpriteToImage();
                Drawing.drawSkeltonMode = false;
            }

            if(PenTool.doLinking){
                link = new Link();
				output = link.Linking();
                DrawTriangulation = !DrawTriangulation;
                ResetCanvas();
                PenTool.doLinking = false;
            }
  
            if(isDrawing){
                   
                // Is the user holding down the left mouse button?
                bool mouse_held_down = Input.GetMouseButton(0);
                
                if (mouse_held_down && !no_drawing_on_current_drag)
                {
                    // Convert mouse coordinates to world coordinates
                    Vector2 mouse_world_position = cam.ScreenToWorldPoint(Input.mousePosition);

                    // Check if the current mouse position overlaps our image
                    Collider2D hit = Physics2D.OverlapPoint(mouse_world_position, Drawing_Layers.value);
                    if (hit != null && hit.transform != null)
                    {
                        // We're over the texture we're drawing on!
                        // Use whatever function the current brush is
                        current_brush(mouse_world_position);
                    }

                    else
                    {
                        // We're not over our destination texture
                        previous_drag_position = Vector2.zero;
                        if (!mouse_was_previously_held_down)
                        {
                            // This is a new drag where the user is left clicking off the canvas
                            // Ensure no drawing happens until a new drag is started
                            no_drawing_on_current_drag = true;
                        }
                    }
                }
                // Mouse is released
                else if (!mouse_held_down)
                {
                    previous_drag_position = Vector2.zero;
                    no_drawing_on_current_drag = false;
                }
                mouse_was_previously_held_down = mouse_held_down;
            }

            if (Drawing.vanishMode){
                points    = new List<Vector2>(); 
                uv        = new List<Vector2> (); 
                triangles = new List<Triangle>(); 
                moveMesh  = false;
                isDrawing = true ;
                DrawTriangulation = false ;
                Destroy(go);
                go = Instantiate(prefab);
                Drawing.vanishMode = false;
            }

            if (changeTexture){

                if(Scroll.spriteIsChanged){
                    drawable_texture.LoadImage(Scroll.spriteToChange);
                    drawable_texture.Apply();
                    drawable_sprite = Sprite.Create(drawable_texture, new UnityEngine.Rect(0, 0, drawable_texture.width, drawable_texture.height), Vector2.zero);
                    drawable_texture = drawable_sprite.texture;
                    Scroll.spriteIsChanged = false;
                }
                changeTexture    = false;

            }

             if (Input.GetKeyDown(KeyCode.D))
            {
                // Do something when "D" key is pressed
            }

        }


        public void ConvertSpriteToImage()
        {
            if (drawable_sprite != null)
            {
                moveMesh = true;
                // Create a new Texture2D with the Sprite's dimensions
                Texture2D texture = new Texture2D((int)drawable_sprite.rect.width, (int)drawable_sprite.rect.height);
                
                // Get the pixels from the drawable_sprite and set them to the Texture2D
                texture.SetPixels(drawable_sprite.texture.GetPixels(
                    (int)drawable_sprite.textureRect.x,
                    (int)drawable_sprite.textureRect.y,
                    (int)drawable_sprite.textureRect.width,
                    (int)drawable_sprite.textureRect.height
                    )
                );
                
                // Apply the changes to the Texture2D
                texture.Apply();

                // Convert the Texture2D to bytes (PNG format)
                byte[] bytes = texture.EncodeToPNG();

                    //    BEGIN find contours from texture 

                        float width  = 13f ;
                        float height = 9.25f;
                        float center_x = width   / 2 ; 
                        float center_y = height  / 2 ;

                        Mat image = Unity.TextureToMat (texture);

                        //Gray scale image
                        Mat grayMat = new Mat();

                        Cv2.CvtColor (image, grayMat, ColorConversionCodes.BGR2GRAY); 

                        Mat thresh = new Mat ();
                        Cv2.Threshold (grayMat, thresh, 250, 255, ThresholdTypes.BinaryInv);

                        // Extract Contours
                        Point[][] contours;
                        HierarchyIndex[] hierarchy;
                        Cv2.FindContours (thresh, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple, null);

                        
                        List<Point[]> largestContours = new List<Point[]>();
                        largestContours = contours.Select(innerArray => innerArray.ToArray()).ToList();

                        // Sort the contours by area in descending order
                        largestContours = largestContours.OrderByDescending(c => Cv2.ContourArea(c)).ToList();

                        // Cv2.DrawContours(image, largestContours, 0, Scalar.Yellow  , 3, LineTypes.Link8);
                        // Cv2.DrawContours(image, largestContours, 1, Scalar.Blue , 3, LineTypes.Link8);
                        // Display the result
                        // Cv2.ImShow("Contours", image);
                        // Cv2.WaitKey(0);
                        // Cv2.DestroyAllWindows();
                        
                        for (int k = 0 ; k < largestContours[0].Length  ; k++){
                            points.Add(
                                new Vector2(
                                    ((  largestContours[0][k].X - center_x ) / 100 ) + 0.05f ,
                                    (( -largestContours[0][k].Y - center_y ) / 100 ) + 0.05f 
                                    )
                                );
                        }

                    //    END  find contours from texture

                    //    BEGIN Triangulation 
                    
                        points = Utils2D.Constrain(points, 0.25f );           // #Threshold
                        var polygon = Polygon2D.Contour(points.ToArray());
                        var vertices = polygon.Vertices;
                        if(vertices.Length < 3) 
                            return; // error
                        var triangulation = new Triangulation2D(polygon, 20); // #TriangulationAngel
                        go = Instantiate(prefab);
                        go.GetComponent<DemoMesh>().SetTriangulation(triangulation);
                        globalMesh = go.GetComponent<DemoMesh>().mesh;
                        
                    //    END   Triangulation 

                    //    BEGIN Texturing 

                        int[] trianglesPoints = globalMesh.triangles;
                        int id = 0 ;

                        UpdateAll[] pointsMimo = new UpdateAll[globalMesh.vertices.Length];
                        PenTool.newVertices = new List<UpdateAll>();
                        PenTool.textureVertices = new Vector3[globalMesh.vertices.Length];

                        uv = new List<Vector2>();
                        for (int i = 0; i < globalMesh.vertices.Length; i++){
                            pointsMimo[i] = new UpdateAll( i , globalMesh.vertices[i] );
                            PenTool.newVertices.Add(pointsMimo[i]);
                            uv.Add(new Vector2(pointsMimo[i].vector.x/width,pointsMimo[i].vector.y/height));
                        }

                        triangles = new List<Triangle>();
                        for (int i = 0; i < trianglesPoints.Length; i += 3){
                            Triangle triangle = new Triangle();
                            triangle.id = id ;
                            triangle.a = pointsMimo[trianglesPoints[i + 0]];
                            triangle.b = pointsMimo[trianglesPoints[i + 1]];
                            triangle.c = pointsMimo[trianglesPoints[i + 2]];
                            triangles.Add(triangle);
                            id = id + 1 ;
                        }

                        globalMesh.vertices = PenTool.textureVertices;
                        globalMesh.uv = uv.ToArray();
                        globalMesh.RecalculateNormals();
                        globalMesh.RecalculateBounds();
                        globalMesh.RecalculateTangents();
                    
                    //    END   Texturing 
                    
                    points.Clear(); 

                // Optionally, you can save the bytes to a file
                // System.IO.File.WriteAllBytes("C:/Users/HP/Downloads/Compressed/trianglation/Assets/OpenCV+Unity/Demo/Identifiy_Contours_by_Shape/someshapes.jpg", bytes);

                // Destroy the temporary Texture2D object
                Destroy(texture);

                // Now you can use the bytes as an image or apply them to a texture
            }
            else
            {
                print("drawable_sprite is not assigned!");
            }
        }

        // Set the colour of pixels in a straight line from start_point all the way to end_point, to ensure everything inbetween is coloured
        public void ColourBetween(Vector2 start_point, Vector2 end_point, int width, Color color)
        {
            // Get the distance from start to finish
            float distance = Vector2.Distance(start_point, end_point);
            Vector2 direction = (start_point - end_point).normalized;

            Vector2 cur_position = start_point;

            // Calculate how many times we should interpolate between start_point and end_point based on the amount of time that has passed since the last update
            float lerp_steps = 1 / distance;

            for (float lerp = 0; lerp <= 1; lerp += lerp_steps)
            {
                cur_position = Vector2.Lerp(start_point, end_point, lerp);
                MarkPixelsToColour(cur_position, width, color);
            }
        }

        public void MarkPixelsToColour(Vector2 center_pixel, int pen_thickness, Color color_of_pen)
        {
            // Figure out how many pixels we need to colour in each direction (x and y)
            int center_x = (int)center_pixel.x;
            int center_y = (int)center_pixel.y;
            //int extra_radius = Mathf.Min(0, pen_thickness - 2);

            for (int x = center_x - pen_thickness; x <= center_x + pen_thickness; x++)
            {
                // Check if the X wraps around the image, so we don't draw pixels on the other side of the image
                if (x >= (int)drawable_sprite.rect.width || x < 0)
                    continue;

                for (int y = center_y - pen_thickness; y <= center_y + pen_thickness; y++)
                {
                    MarkPixelToChange(x, y, color_of_pen);
                }
            }
        }

        public void MarkPixelToChange(int x, int y, Color color)
        {
            // Need to transform x and y coordinates to flat coordinates of array
            int array_pos = y * (int)drawable_sprite.rect.width + x;

            // Check if this is a valid position
            if (array_pos > cur_colors.Length || array_pos < 0)
                return;

            cur_colors[array_pos] = color;
        }

        public void ApplyMarkedPixelChanges()
        {
            drawable_texture.SetPixels32(cur_colors);
            drawable_texture.Apply();
        }

        // Directly colours pixels. This method is slower than using MarkPixelsToColour then using ApplyMarkedPixelChanges
        // SetPixels32 is far faster than SetPixel
        // Colours both the center pixel, and a number of pixels around the center pixel based on pen_thickness (pen radius)
        public void ColourPixels(Vector2 center_pixel, int pen_thickness, Color color_of_pen)
        {
            // Figure out how many pixels we need to colour in each direction (x and y)
            int center_x = (int)center_pixel.x;
            int center_y = (int)center_pixel.y;
            //int extra_radius = Mathf.Min(0, pen_thickness - 2);

            for (int x = center_x - pen_thickness; x <= center_x + pen_thickness; x++)
            {
                for (int y = center_y - pen_thickness; y <= center_y + pen_thickness; y++)
                {
                    drawable_texture.SetPixel(x, y, color_of_pen);
                }
            }

            drawable_texture.Apply();
        }

        public Vector2 WorldToPixelCoordinates(Vector2 world_position)
        {
            // Change coordinates to local coordinates of this image
            Vector3 local_pos = transform.InverseTransformPoint(world_position);

            // Change these to coordinates of pixels
            float pixelWidth = drawable_sprite.rect.width;
            float pixelHeight = drawable_sprite.rect.height;
            float unitsToPixels = pixelWidth / drawable_sprite.bounds.size.x * transform.localScale.x;

            // Need to center our coordinates
            float centered_x = local_pos.x * unitsToPixels + pixelWidth / 2;
            float centered_y = local_pos.y * unitsToPixels + pixelHeight / 2;

            // Round current mouse position to nearest pixel
            Vector2 pixel_pos = new Vector2(Mathf.RoundToInt(centered_x), Mathf.RoundToInt(centered_y));

            return pixel_pos;
        }

        // Changes every pixel to be the reset colour
        public void ResetCanvas()
        {
            drawable_texture.LoadImage(File.ReadAllBytes(Home.whiteBoardPath));
            drawable_texture.Apply();
            drawable_sprite  = Sprite.Create(drawable_texture, new UnityEngine.Rect(0, 0, drawable_texture.width, drawable_texture.height), Vector2.zero);
            drawable_texture = drawable_sprite.texture;
        }

        void Awake()
        {

            cam = Camera.main;
            drawable = this;
            // DEFAULT BRUSH SET HERE
            current_brush = PenBrush;

            drawable_sprite = this.GetComponent<SpriteRenderer>().sprite;
            drawable_texture = drawable_sprite.texture;

            // Initialize clean pixels to use
            clean_colours_array = new Color[(int)drawable_sprite.rect.width * (int)drawable_sprite.rect.height];
            for (int x = 0; x < clean_colours_array.Length; x++)
                clean_colours_array[x] = Reset_Colour;

            // Should we reset our canvas image when we hit play in the editor?
            if (Reset_Canvas_On_Play)
                ResetCanvas();
        }
    }
}
