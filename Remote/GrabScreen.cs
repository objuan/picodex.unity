using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// ----------
/// GrabScreen - Command Buffer script
/// This script creates two global textures containing the screen. Blurred and non-blurred.
/// These textures can be used by e.g. refraction shaders to work without a grab pass.
/// Inspired & adapted by: https://blogs.unity3d.com/2015/02/06/extending-unity-5-rendering-pipeline-command-buffers/
/// 
/// INSTRUCTIONS: Put this script on the camera you want to capture the screen from.
/// ----------
/// </summary>
[ RequireComponent(typeof(Camera))] // ExecuteInEditMode,
public class GrabScreen : MonoBehaviour
{

    /// <summary>
    /// The shader used to blur the screen.
    /// </summary>
    public Shader blurShader;

    /// <summary>
    /// Strings used for shader property identification...
    /// </summary>
    private const string
        cBufferName = "GrabAndBlurScreen",
     
        blurShaderKey = "BrickGame/ImageCopyShader";

    /// <summary>
    /// Which camera event the command buffer should be attached to.
    /// </summary>
    private CameraEvent _cameraEvent = CameraEvent.AfterEverything;// CameraEvent.AfterImageEffectsOpaque;
    /// <summary>
    /// The texture filter mode of the created render textures.
    /// </summary>
    private FilterMode _filterMode = FilterMode.Bilinear;
    /// <summary>
    /// Reference to the material we are using
    /// </summary>
    private Material _material;
    /// <summary>
    /// Screen resolution. Used to check if the screen size changed.
    /// </summary>
    private Resolution _currentScreenRes = new Resolution();
    /// <summary>
    /// Reference to our command buffer.
    /// </summary>
    private CommandBuffer _cBuffer;
    /// <summary>
    /// reference to our attached camera.
    /// </summary>
    private Camera _camera;


    public RenderTexture cameraBuffer;

    /// <summary>
    /// Attached camera getter (read only)
    /// </summary>
    private Camera Camera
    {
        get
        {
            GetCamera();
            return _camera;
        }
    }

    /// <summary>
    /// Creates the command buffer.
    /// </summary>
    private void CreateCommandBuffer()
    {
        DestroyCommandBuffer();
        Initialize();

        // Create CommandBuffer
        _cBuffer = new CommandBuffer();
        _cBuffer.name = cBufferName;
        _cBuffer.Clear();

        // _cBuffer.SetRenderTarget(cameraBuffer);

        _cBuffer.Blit(BuiltinRenderTextureType.CurrentActive, cameraBuffer, _material, 0);// new Vector2(0.5f, 0.5f), new Vector2(0,0));

 
        Camera.AddCommandBuffer(_cameraEvent, _cBuffer);
    }

    /// <summary>
    /// Destroys the command buffer.
    /// </summary>
    private void DestroyCommandBuffer()
    {
        if (_cBuffer != null)
        {
            Camera.RemoveCommandBuffer(_cameraEvent, _cBuffer);
            _cBuffer.Clear();
            _cBuffer.Dispose();
            _cBuffer = null;
        }

        // Make sure we don't have any duplicates of our command buffer.
        CommandBuffer[] commandBuffers = Camera.GetCommandBuffers(_cameraEvent);
        foreach (CommandBuffer cBuffer in commandBuffers)
        {
            if (cBuffer.name == cBufferName)
            {
                Camera.RemoveCommandBuffer(_cameraEvent, cBuffer);
                cBuffer.Clear();
                cBuffer.Dispose();
            }
        }
    }

    // Update is called once per frame
    private void OnPreRender()
    {
        //Recreate command buffer if screen size, downsample or blur was (de)activated
        if (_currentScreenRes.height != Camera.pixelHeight || _currentScreenRes.width != Camera.pixelWidth)
        {
            SetCurrentValues();
            CreateCommandBuffer();
        }
        SetCurrentValues();
    }

    /// <summary>
    /// Sets the current values.
    /// </summary>
    private void SetCurrentValues()
    {
        _currentScreenRes.height = Camera.pixelHeight;
        _currentScreenRes.width = Camera.pixelWidth;
    }

    /// <summary>
    /// Initialize stuff.
    /// </summary>
    private void OnEnable()
    {
    //    Initialize();
    }

    /// <summary>
    /// Initialize shader, material, camera 
    /// </summary>
    public void Initialize()
    {
        // If the blur shader isn't initialized, try to find it.
        if (!blurShader)
        {
            blurShader = Shader.Find(blurShaderKey);
        }

        // if no material was created with the blur shader yet, create one.
        if (!_material)
        {
            _material = new Material(blurShader);
            _material.hideFlags = HideFlags.HideAndDontSave;
        }

     

        // convert shader property keywords into IDs.
  
        // get our attached camera.
        GetCamera();

       // float cameraScale = 0.25f;
     //   cameraBuffer = RenderTexture.GetTemporary((int)(_camera.pixelWidth * cameraScale), (int)(_camera.pixelHeight * cameraScale), 24);

    }
    
    /// <summary>
    /// Gets the attached camera.
    /// </summary>
    private void GetCamera()
    {
        if (!_camera)
        {
            _camera = GetComponent<Camera>();
        }
    }
}