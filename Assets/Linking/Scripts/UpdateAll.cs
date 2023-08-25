using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class UpdateAll
{
    public int id ;
    public Vector3 vector;

    public UpdateAll(int id ,Vector3 vector){
        this.id = id;
        this.vector = vector;
    }
    
    public override int GetHashCode() {
        return vector.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;

        UpdateAll other = (UpdateAll)obj;
        return vector == other.vector ;
    }
}
