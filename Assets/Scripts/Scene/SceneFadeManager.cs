using UnityEngine;
using UnityEngine.UI;
public class SceneFadeManager : MonoBehaviour
{
    public static SceneFadeManager instance;

    [SerializeField] public Image _fadeOutImage;
    
    [Range(0.1f, 10f), SerializeField] private float _fadeOutSpeed = 5f;
    [Range(0.1f, 10f), SerializeField] private float _fadeInSpeed = 5f;

    [SerializeField] private Color _fadeOutStartColor;
 
    public bool IsFadingOut { get; private set; }

    public bool IsFadingIn { get; private set; }



    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        else
        {
            Destroy(gameObject);
        }
        _fadeOutStartColor.a = 0f;
   
    }

    private void Update()
    {
        if (IsFadingOut)
        {
           
            if (_fadeOutImage.color.a < 1f)
            {
                _fadeOutStartColor.a += Time.deltaTime * _fadeOutSpeed;
                _fadeOutImage.color = _fadeOutStartColor;
            }
            else
            {
                IsFadingOut = false;
            }
        }
        

        if (IsFadingIn)
        {
          
            float delta = Time.deltaTime;
            delta = Mathf.Min(delta, 0.016f);

            if (_fadeOutImage.color.a > 0f)
            {
                _fadeOutStartColor.a -= delta * _fadeInSpeed;
                _fadeOutImage.color = _fadeOutStartColor;
            }
            else
            {
                IsFadingIn = false;
                ReturnSpeedSettings();
            }
        }
     
    }
    public void StartFadeOut()
    {   
        _fadeOutImage.color = _fadeOutStartColor;
        IsFadingOut = true;
    }
    public void StartFadeIn()
    {
        
        if (_fadeOutImage.color.a >= 1f)
        {
            _fadeOutImage.color = _fadeOutStartColor;
            IsFadingIn = true;
        }
        
    }
    public void ChangeSpeedSettings(float fadeoutS, float fadeinS)
    {      
        _fadeOutSpeed = fadeoutS;
        _fadeInSpeed = fadeinS;
    }
    public void ReturnSpeedSettings()
    {
        _fadeOutSpeed = 5f;
        _fadeInSpeed = 5f;
    }
}
