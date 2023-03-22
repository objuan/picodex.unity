using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BezierNode
{
    public Vector3 position = Vector3.zero;
    public Quaternion rotation = Quaternion.identity;
}

public class BezierNodeIterator : BezierNode
{
    BezierCurve curve;
    float space = 0;

    public Vector3 direction;
    Vector3 lastP = Vector3.zero;

    public BezierNodeIterator(BezierCurve curve)
    {
        this.curve = curve;
    }

    public bool Next(float speed, float timeDelta)
    {
        float sp = speed * timeDelta;
        space += sp;
        if (space < curve.curveLen)
        {
            position = curve.GetAtSpace(space).position;
            direction = (position - lastP).normalized;
            lastP = position;
            return true;
        }
        else
        {
            try
            {
                position = curve.linePointList[curve.linePointList.Count - 1];
            }
            catch (System.Exception) { }
            //direction = (position - lastP).normalized;
            return false;
        }
    }
}

public class BezierCurve 
{

    public List<BezierNode> wayPoint = new List<BezierNode>(); //Road point information (head and tail indicate start and end points, middle is relative to nth order offset point)
    public int pointCount = 100; //number of points on the curve
    public List<Vector3> linePointList;
    [Range(0, 1)]
    public float _time = 0.01f; //motion interval between two points

    public float curveLen;

    public Vector3 ExitDir => ( linePointList[linePointList.Count - 1] - linePointList[linePointList.Count - 2]).normalized;

    public Transform player; //moving object
 //   public Transform targetTransform; //Play target object

    private bool isMove = false;
    
    float _curTimer = 0.0f; //time
    int lineItem = 1; //target index


    public void AddNode(BezierNode node)
    {
        wayPoint.Add(node);
        Update();
    }

    public void AddNode(Vector3 pos)
    {
        wayPoint.Add(new BezierNode() { position = pos, rotation = Quaternion.identity });
      //  Update();
    }
    public void AddNode(Vector3 pos, Quaternion rot)
    {
        wayPoint.Add(new BezierNode() { position = pos, rotation = rot });
        //  Update();
    }
    public void AddNode(Transform trx,Vector3 offset )
    {
        wayPoint.Add(new BezierNode() { position = trx.position+ offset, rotation = trx.rotation });
        //  Update();
    }
    public void AddNode(Transform trx)
    {
        wayPoint.Add(new BezierNode() { position = trx.position , rotation = trx.rotation });
        //  Update();
    }
    public void AddNode_Middle(Transform from, Transform to,Vector3 offset)
    {
        wayPoint.Add(new BezierNode() { position = offset+ Vector3.Lerp( from.position, to.position, 0.5f) , rotation = Quaternion.Slerp(from.rotation, to.rotation , 0.5f) });
        //  Update();
    }
    // Update is called once per frame
    public void Update()
    {
        if (!isMove) return;
        _curTimer += Time.deltaTime;
        if (_curTimer > _time)
        {
            _curTimer = 0;

            //if (targetTransform)
            //    player.LookAt(targetTransform);
            //else
          //      player.LookAt(linePointList[lineItem]);

            player.localPosition = Vector3.Lerp(linePointList[lineItem - 1], linePointList[lineItem], 1f);

            lineItem++;
            if (lineItem >= linePointList.Count)
                lineItem = 1;
        }
    }
    // linear
    Vector3 Bezier(Vector3 p0, Vector3 p1, float t)
    {
        return (1 - t) * p0 + t * p1;
    }
    // second order curve
    Vector3 Bezier(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        Vector3 p0p1 = (1 - t) * p0 + t * p1;
        Vector3 p1p2 = (1 - t) * p1 + t * p2;
        Vector3 result = (1 - t) * p0p1 + t * p1p2;
        return result;
    }
    // third-order curve
    Vector3 Bezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        Vector3 result;
        Vector3 p0p1 = (1 - t) * p0 + t * p1;
        Vector3 p1p2 = (1 - t) * p1 + t * p2;
        Vector3 p2p3 = (1 - t) * p2 + t * p3;
        Vector3 p0p1p2 = (1 - t) * p0p1 + t * p1p2;
        Vector3 p1p2p3 = (1 - t) * p1p2 + t * p2p3;
        result = (1 - t) * p0p1p2 + t * p1p2p3;
        return result;
    }

    // n-order curve, recursive implementation
    public Vector3 Bezier(float t, List<Vector3> p)
    {
        if (p.Count < 2)
            return p[0];
        List<Vector3> newp = new List<Vector3>();
        for (int i = 0; i < p.Count - 1; i++)
        {
           // Debug.DrawLine(p[i], p[i + 1], Color.yellow);
            Vector3 p0p1 = (1 - t) * p[i] + t * p[i + 1];
            newp.Add(p0p1);
        }
        return Bezier(t, newp);
    }
    // Transform is converted to vector3, and the Bezier function with parameter List<Vector3> is called.
    public Vector3 Bezier(float t, List<BezierNode> p)
    {
        if (p.Count < 2)
            return p[0].position;
        List<Vector3> newp = new List<Vector3>();
        for (int i = 0; i < p.Count; i++)
        {
            newp.Add(p[i].position);
        }
        //return Bezier(t, newp);
        return MyBezier(t, newp);
    }
    //Draw an arc
    public Vector3 MyBezier(float t, List<Vector3> p)
    {
        if (p.Count < 2)
            return p[0];
        List<Vector3> newp = new List<Vector3>();
        for (int i = 0; i < p.Count - 1; i++)
        {
            //Debug.DrawLine(p[i], p[i + 1], Color.yellow);
            Vector3 p0p1 = (1 - t) * p[i] + t * p[i + 1];
            newp.Add(p0p1);
        }
        return MyBezier(t, newp);
    }

    void Init()
    {
        if (linePointList == null)
        {
            curveLen = 0;
            linePointList = new List<Vector3>();
            for (int i = 0; i < pointCount; i++)
            {
                var point = Bezier(i / (float)pointCount, wayPoint);
            
                if (i>0)
                    curveLen+=  (point - linePointList[linePointList.Count-1]).magnitude;

                linePointList.Add(point);
            }
            if (linePointList.Count == pointCount)
                isMove = true;
        }
        //Debug.LogError("isMove == " + isMove);
    }

    public BezierNode GetAt(float t)
    {
        try
        {
            BezierNode n = new BezierNode();
            Init();

            if (t >= 1)
                n.position = linePointList[linePointList.Count - 1];
            else
            {

                int prev = (int)Mathf.FloorToInt(((float)(linePointList.Count-1) * t));
                float sub_t = t - (1f / linePointList.Count) * prev;

                n.position = Vector3.Lerp(linePointList[prev], linePointList[prev + 1], sub_t);
            }
            return n;
        }
        catch (System.Exception e)
        {
            int y = 0;
            return null;
        }
    }
    public BezierNode GetAtSpace(float sp)
    {
        return GetAt(sp / curveLen);
    }

    public BezierNodeIterator Interate()
    {
        return new BezierNodeIterator(this);
    }
      
    // Display in the scene view
       
    public void OnDrawGizmos()
    {
        Init();
        Gizmos.color = Color.yellow;
        //Gizmos.DrawLine()
        for (int i = 0; i < linePointList.Count - 1; i++)
        {
            //var point_1 = Bezier(i/(float)pointCount, wayPoint);
            //var point_2 = Bezier((i+1) / (float)pointCount, wayPoint);
            //Two lines can be used
            //Gizmos.DrawLine(point_1, point_2);
            Debug.DrawLine(linePointList[i], linePointList[i + 1], Color.yellow);
        }

    }

}