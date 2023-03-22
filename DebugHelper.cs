
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugElement
{
    public string name = "";
    public DebugHelper helper;
    public GameObject go;
    public Material material;
    public Color startColor = Color.red;
    public Color endColor = Color.yellow;

    public bool enable = true;

    public virtual void Update()
    {
    }

    public void Destroy()
    {
        helper.Delete(this);
    }
}

public class DebugLine : DebugElement
{
   
    public Vector3 start;
    public Vector3 end;
    public float size = 1;
    GameObject cy;

    public void Set(Vector3 start, Vector3 end)
    {
        this.start = start;
        this.end = end;
    }

    public void SetMaterial(Material material)
    {
        cy.GetComponent<MeshRenderer>().material = material;
    }

    public override  void Update()
    {
        if (!go)
        {
            go = new GameObject("line");
            go.SetParentAtOrigin(helper.gameObject);
            cy = PrimitiveManager.CreateCylinderObject(go, size, 1, 16);
            cy.GetComponent<MeshRenderer>().material = material;
            cy.transform.rotation = Quaternion.Euler(90, 0, 0);
            MeshUtils.SetPivot(cy.GetComponent<MeshFilter>(), new Vector3(0,-0.5f,0));
        }
        go.transform.position = start;
        go.transform.LookAt(end);

        cy.transform.localScale = new Vector3(1,  (end - start).magnitude,1);
        go.SetActive( enable);
    }
}

public enum DebugLineRenderMode
{
    Solid,
    Dotted,
    DottedAnimated
}

public class DebugPolyLine : DebugElement
{
    public int? renderQueue = null;
    float txt_offset= 1024;
    public Vector3[] points;
    public float startWidth = 1;
    public float endWidth = 1;
    DebugLineRenderMode mode = DebugLineRenderMode.Solid;
    private LineRenderer lineRenderer;
  

    public DebugLineRenderMode Mode { get { return mode; }
        set {
            mode = value;
          
        }
    }

    public float size
    {
        get { return startWidth; }
        set
        {
            startWidth = endWidth = value;
        }
    }
   
    public void Set(params Vector3[] points)
    {
        this.points = points;
    }

    public void SetMaterial(Material material)
    {
        this.material = material;
        if (lineRenderer)
            lineRenderer.GetComponent<MeshRenderer>().material = material;
    }

    public void SetColor(Color startColor,Color endColor)
    {
      //  if (go && (!startColor.Equals(this.startColor) || endColor.Equals(this.endColor)))
        {
            this.startColor = startColor;
            this.endColor = endColor;
            if (lineRenderer != null)
            {
                lineRenderer.startColor = startColor;
                lineRenderer.endColor = endColor;
            }
        }
    }

    //public void Set(Vector3 from, Vector3 to)
    //{
    //    this.points = new Vector3[] { from, to }};
    //}

    public override void Update()
    {
        if (!go)
        {
            go = new GameObject(name!="" ? name : "polyline");
            go.SetParentAtOrigin(helper.gameObject);
            lineRenderer = go.AddComponent<LineRenderer>();
            lineRenderer.receiveShadows = false;
            lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lineRenderer.GetComponent<LineRenderer>().material = material;
            lineRenderer.startColor = startColor;
            lineRenderer.endColor = endColor;
            lineRenderer.startWidth = startWidth;
            lineRenderer.endWidth = endWidth;
         
            if (mode != DebugLineRenderMode.Solid)
            {
                lineRenderer.GetComponent<LineRenderer>().material = helper.lineDottedMaterial;
                lineRenderer.textureMode = LineTextureMode.Tile;
                //size = 0.025f;
                //lineRenderer.startWidth = startWidth;
                //lineRenderer.endWidth = endWidth;

            }
            if (renderQueue.HasValue)
                lineRenderer.material.renderQueue = renderQueue.Value;

        }
        if (points != null)
        {
            if (lineRenderer.positionCount != points.Length) lineRenderer.positionCount = (points.Length);
            lineRenderer.SetPositions(points);

            if (mode != DebugLineRenderMode.Solid)
            {
                float width = lineRenderer.startWidth;

                 lineRenderer.material.mainTextureScale = new Vector2(1f / width, 1.0f);

                if (mode == DebugLineRenderMode.DottedAnimated)
                {
                    txt_offset -= Time.time * 0.01f;
                    if (txt_offset < 0) txt_offset = 1024; 
                    lineRenderer.material.mainTextureOffset = new Vector2(txt_offset, 0);
                }
            }
        }
        lineRenderer.enabled = enable;
    }
}

public class DebugLinePointer : DebugPolyLine
{
    GameObject hitPointGO;
    Vector3 endNormal;

    public void Set(Vector3 start, Vector3 end, Vector3 endNormal)
    {
        this.points = new Vector3[] {start,end };
        this.endNormal = endNormal;
    }
    public override void Update()
    {
        base.Update();

        if (!hitPointGO)
        {
            hitPointGO = PrimitiveManager.CreateSphereObject(go, startWidth);
            hitPointGO.GetComponent<MeshRenderer>().material = material;
        }
        if (points != null)
        {
            hitPointGO.transform.position = points[points.Length - 1];

            hitPointGO.SetActive(enable);
        }
    }
}

public class DebugLabel : DebugElement
{
    public GameObject labelObject;
    public GameObject target;
    MeshFilter m;
    string text = "";
    Camera camera;
    Vector3 manualPos = Vector3.zero;

    public override void Update()
    {
        if (!go)
        {
            go = GameObject.Instantiate(labelObject, target.transform);
            m = target.GetComponent<MeshFilter>();
            if (m)
            {
                go.transform.localPosition = new Vector3(0, m.mesh.bounds.size.y,0);
            }
            else
                go.transform.localPosition = new Vector3(0,0.1f, 0);
            // go.AddComponent<Text
            SetText(text);

            camera = Camera.main;
        }
        if (manualPos == Vector3.zero)
        {
            if (m)
            {
                go.transform.position = target.transform.position + new Vector3(0, m.mesh.bounds.size.y, 0);
            }
            else
                go.transform.position = target.transform.position + new Vector3(0, 0.1f, 0);

            go.transform.rotation = Quaternion.identity;
        }
        else
        {
            go.transform.rotation = camera.transform.rotation;
            go.transform.position = manualPos+ camera.transform.up * 0.01f+ -camera.transform.forward*0.01f;
        }
    }
    public void SetPosition(Vector3 pos)
    {
        manualPos = pos;
    }

    public void SetText(string msg)
    {
        text = msg;
        if (go && go.GetComponentInChildren<Text>())
            go.GetComponentInChildren<Text>().text = msg;
        //if (go && go.GetComponent<TextMesh>())
        //    go.GetComponent<TextMesh>().text = msg;
    }

   
}


// ===================

public class DebugHelperRef : MonoBehaviour
{
    public DebugElement element;
}

public class DebugHelper : MonoBehaviour
{
    List<DebugElement> list = new List<DebugElement>();
    public Material lineMaterial;
    public Material lineDottedMaterial;
    public GameObject labelObject;

    public DebugLine CreateLine(float size = 0.02f)
    {
        DebugLine line = new DebugLine();
        line.helper = this;
        line.size = size;
        line.material = lineMaterial;
        list.Add(line);
        return line;
    }

    public DebugLinePointer CreateLinePointer(float size = 0.005f)
    {
        DebugLinePointer line = new DebugLinePointer();
        line.helper = this;
        line.material = lineMaterial;
        line.startWidth = size;
        line.endWidth = size/2;
        line.enable = false;
        line.name = "line pointer";
        list.Add(line);
        return line;
    }

    public DebugPolyLine CreatePolyLine(string name,float size= 0.02f)
    {
        DebugPolyLine line = new DebugPolyLine();
        line.name = "py_"+name;
        line.helper = this;
        line.material = lineMaterial;
        line.size = size;
        line.enable = false;
        list.Add(line);
        return line;
    }

    public DebugLabel CreateLabel(GameObject go)
    {
        DebugLabel line = new DebugLabel();
        line.helper = this;
        line.material = lineMaterial;
        list.Add(line);
        line.target = go;
        line.labelObject = labelObject;
        return line;
    }

    public void Delete(DebugElement ele)
    {
        if (ele.go) GameObject.Destroy(ele.go);
        list.Remove(ele);
    }

    
    private void Update()
    {
        foreach (var ele in list) ele.Update();
    }
}
