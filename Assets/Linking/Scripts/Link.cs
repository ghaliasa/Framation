using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Framation;
using mattatz.Triangulation2DSystem;
using mattatz.Triangulation2DSystem.Example;
using Random = UnityEngine.Random;

public class Link : MonoBehaviour {

    private List<Triangle> triangles ;
    private List<Skeleton> skeletons;
    private Dictionary<(float, float, float), List<int>> pointTrianglesCommon ;

    public Link(){
        triangles = Drawable.drawable.triangles;
        skeletons = PenTool.skeletons ;
        pointTrianglesCommon = new Dictionary<(float, float, float), List<int>>();
    }

    public Dictionary<LineController, List<Triangle>> Linking(){

        Skeleton skeleton = skeletons[0];

        // This dictionary will relate each point with its triangles
        // (float, float, float)  ==> coordinates of point from heads of any triangle
        // List<int>              ==> list of triangles id that have previous point as one of its vertices
        Dictionary<(float, float, float), List<int>> pointTriangles = new Dictionary<(float, float, float), List<int>>();

        // We will fill this dictionary for all the points and their triangles
        // by adding each triangle to its 3 points (a, b, c)
        foreach (Triangle triangle in triangles)
        {
            // Initialize the lists for the points if they don't exist
            if (!pointTriangles.ContainsKey((triangle.a.vector.x, triangle.a.vector.y, triangle.a.vector.z)))
                pointTriangles[(triangle.a.vector.x, triangle.a.vector.y, triangle.a.vector.z)] = new List<int>();
            if (!pointTriangles.ContainsKey((triangle.b.vector.x, triangle.b.vector.y, triangle.b.vector.z)))
                pointTriangles[(triangle.b.vector.x, triangle.b.vector.y, triangle.b.vector.z)] = new List<int>();
            if (!pointTriangles.ContainsKey((triangle.c.vector.x, triangle.c.vector.y, triangle.c.vector.z)))
                pointTriangles[(triangle.c.vector.x, triangle.c.vector.y, triangle.c.vector.z)] = new List<int>();

            // Add the triangle to the corresponding points
            pointTriangles[(triangle.a.vector.x, triangle.a.vector.y, triangle.a.vector.z)].Add(triangle.id);
            pointTriangles[(triangle.b.vector.x, triangle.b.vector.y, triangle.b.vector.z)].Add(triangle.id);
            pointTriangles[(triangle.c.vector.x, triangle.c.vector.y, triangle.c.vector.z)].Add(triangle.id);
        }

        pointTrianglesCommon = pointTriangles ;


        // Now we know all triangles that touch a specific point
        // and now we can know the neighbors of a triangle by knowing the neighbors of all its points
        // so we will calculate them and put them in the neighbors dictionary

        // Define the neighbors dictionary and initialize it as empty
        // Dictionary<1,2>  : 1 - id of triangle 
        //                    2 - list of its neighbors  
        Dictionary<int, List<int>> neighbors = new Dictionary<int, List<int>>();

        // Fill the neighbors dictionary by iterating over all the triangles
        // We will add each neighbor for any point of this triangle as a neighbor for it
        foreach (Triangle triangle in triangles)
        {
            neighbors[triangle.id] = new List<int>();

            // Add the neighbors of point a
            foreach (int neighborId in pointTriangles[(triangle.a.vector.x, triangle.a.vector.y, triangle.a.vector.z)]){
                if (!neighbors[triangle.id].Contains(neighborId))
                    neighbors[triangle.id].Add(neighborId);
            }

            // Add the neighbors of point b
            foreach (int neighborId in pointTriangles[(triangle.b.vector.x, triangle.b.vector.y, triangle.b.vector.z)]){
                if (!neighbors[triangle.id].Contains(neighborId))
                    neighbors[triangle.id].Add(neighborId);
            }

            // Add the neighbors of point c
            foreach (int neighborId in pointTriangles[(triangle.c.vector.x, triangle.c.vector.y, triangle.c.vector.z)]){
                if (!neighbors[triangle.id].Contains(neighborId))
                    neighbors[triangle.id].Add(neighborId);
            }
        }

        // Now we have the neighbors dictionary filled correctly
        // and by it, we can access all the neighbors for a specific triangle
        // neighbors[triangle] -> List<Triangle>

        // This dictionary will give us in the end the distance of each triangle for each line
        // Dictionary<(1, 2), 3>  : 1 - line id
        //                          2 - triangle id
        //                          3 - distance between line and triangle
        Dictionary<(int, int), int> distance = new Dictionary<(int, int), int>();

        // We will fill this distance with -1 (-1 means that this value is not calculated yet)
        foreach (LineController line in skeleton.lines)
        {
            foreach (Triangle triangle in triangles)
            {
                distance[(line.id, triangle.id)] = -1;
            }
        }


        // Find Direct Connected Triangles For Each Line
        // Dictionary<1, List<Triangle>>  :  1 - line id  
        Dictionary<int, List<Triangle>> directConnectedTriangles = new Dictionary<int, List<Triangle>>();

        for (int i = 0; i < skeleton.lines.Count; i ++){
            directConnectedTriangles[skeleton.lines[i].id] = new List<Triangle>();
            for (int j = 0; j < triangles.Count; j ++){
                if (
                        Utils2D.Intersect(
                                skeleton.lines[i].start.transform.position,skeleton.lines[i].end.transform.position,
                                triangles[j].a.vector,triangles[j].b.vector
                            )
                        ||
                        Utils2D.Intersect(
                            skeleton.lines[i].start.transform.position,skeleton.lines[i].end.transform.position,
                            triangles[j].b.vector,triangles[j].c.vector
                            )
                        ||
                        Utils2D.Intersect(
                            skeleton.lines[i].start.transform.position,skeleton.lines[i].end.transform.position,
                            triangles[j].a.vector,triangles[j].c.vector
                            )
                    ) {
                        directConnectedTriangles[skeleton.lines[i].id].Add(triangles[j]);
                    }
            }
        }

        // Now we have to calculate the direct triangle for each line
        // Then we will do BFS for each line
        // In every BFS, we will calculate the distance of all the triangles for a specific line

        // Our BFS will be multi-source (that means we will start with a queue that has many nodes)
        // When we start this BFS, we will fill the queue with the direct triangles with 0 distance
        foreach (LineController line in skeleton.lines)
        {   
            // Queue<KeyValuePair<1, 2>> : 1 - triangle id
            //                             2 - distance between triangle and current processing line
            Queue<KeyValuePair<int, int>> queue = new Queue<KeyValuePair<int, int>>();

            // Iterate over all the triangles that are directly connected with the temporary line
            // Set their distance as 0 and add them to the queue
            foreach (Triangle triangle in directConnectedTriangles[line.id])
            {
                distance[(line.id, triangle.id)] = 0;
                queue.Enqueue(new KeyValuePair<int, int>(triangle.id, 0));
            }

            while (queue.Count > 0)
            {
                // Get the first element of the queue
                KeyValuePair<int, int> current = queue.Dequeue();

                foreach (int neighborId in neighbors[current.Key])
                {
                    // Check if we calculated this neighbor before or not
                    // If it is not calculated, we have to calculate it
                    // If it is already calculated, we will skip it
                    if (distance[(line.id, neighborId)] == -1)
                    {
                        // Calculate the distance for this neighbor
                        distance[(line.id, neighborId)] = current.Value + 1;
                        // Add it to the queue to calculate its neighbors as well
                        queue.Enqueue(new KeyValuePair<int, int>(neighborId, current.Value + 1));
                    }
                }
            }
        }

        // Now, after the BFS is finished, we will have the distance dictionary filled with the correct values
        // We will be able to know the distance between each line and triangle (0 means direct touch)

        // Now we can relate the triangles to their lines in the output
        // Define the epsilon or take it as an argument of the function or access it by any way
        int epsilon = 0;

        // When epsilon == 0, that means we will add each triangle to the closest line
        // and we will add it to many lines if they are with the same closest distance
        // Ex: distances=[2, 2, 2, 4, 5]
        // In this example, we will add the triangle to the first 3 lines with a distance of 2

        // Define a dictionary that gives us all the triangles for a line
        Dictionary<LineController, List<Triangle>> lineTriangles = new Dictionary<LineController, List<Triangle>>();

        foreach (Triangle triangle in triangles)
        {
            int minimumDistance = int.MaxValue;

            // Calculate the minimum distance to compare with other distances
            foreach (LineController line in skeleton.lines)
            {
                minimumDistance = Math.Min(minimumDistance, distance[(line.id, triangle.id)]);
            }

            // Compare the distances with the minimumDistance
            // If the distance - minimumDistance <= epsilon the  for this line,
            // then we will consider it as connected to this triangle
            foreach (LineController line in skeleton.lines)
            {
                if (distance[(line.id, triangle.id)] - minimumDistance <= epsilon)
                {
                    if (!lineTriangles.ContainsKey(line))
                        lineTriangles[line] = new List<Triangle>();
                    triangle.lines.Add(line);
                    lineTriangles[line].Add(triangle);
                    break;
                }
            }
        }

        // Now we have the lineTriangles dictionary filled
        // and we can access the triangles for a specific line directly through this dictionary
        // You can fill the output as needed

        /// ---------- here you have to fill the output from the lineTriangles dictionary --------- ///

        foreach (KeyValuePair<LineController, List<Triangle>> kvp in lineTriangles){
            Color coco = new Color(Random.value, Random.value, Random.value, 1.0f);
            foreach(Triangle triangle in kvp.Value){
                triangle.color = coco;
            }
        }
        return lineTriangles;
    }

}