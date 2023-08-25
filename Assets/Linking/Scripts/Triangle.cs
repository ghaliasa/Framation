using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using Framation;
using OperationNamespace;

public class Triangle  : MonoBehaviour {

    public int id ;
    public UpdateAll a,b,c;
    public List<LineController> lines;
    public Color color;
    public Transform triangleTransform;

    public Triangle(){
        lines = new List<LineController>();
    }

    public void Shift(Vector3 center,Operation operation){
        if(operation == Operation.Minus) {
            this.a.vector = this.a.vector - center ; 
            this.b.vector = this.b.vector - center ; 
            this.c.vector = this.c.vector - center ;
        }
        if(operation == Operation.Add) {
            this.a.vector = this.a.vector + center ; 
            this.b.vector = this.b.vector + center ; 
            this.c.vector = this.c.vector + center ;
        }
    }

    public void TransformationSTR( Vector3 scale , Vector3 translate , float rotation){
        this.a.vector = Vector3.Scale(this.a.vector , scale);
        this.b.vector = Vector3.Scale(this.b.vector , scale);
        this.c.vector = Vector3.Scale(this.c.vector , scale);

        this.a.vector = this.a.vector + translate ;
        this.b.vector = this.b.vector + translate ;
        this.c.vector = this.c.vector + translate ;
        
        this.a.vector = Quaternion.Euler(0f, 0f,  rotation) * this.a.vector ;
        this.b.vector = Quaternion.Euler(0f, 0f,  rotation) * this.b.vector ;
        this.c.vector = Quaternion.Euler(0f, 0f,  rotation) * this.c.vector ;
    }
    
}