using UnityEngine;
using TMPro;

namespace Towerino
{
    // Main title menu scene controller
    public class MenuController : MonoBehaviour
    {
        [SerializeField]
        private GameObject _camera = null;
        [SerializeField]
        private RectTransform _firstTimeFader = null;
        [SerializeField]
        private RectTransform _title = null;
        [SerializeField]
        private TMP_Text _version = null;

        private bool _tweening;

        // We start with a FadeOut
        private void Awake()
        {
            GameMaster.Instance.Fader.FadeOut();

            if (GameMaster.Instance.GameInitialized) Destroy(_firstTimeFader.gameObject);
            else
            {
                // If this is the first time we run the game (it is not coming back from another scene)
                // we make an fade in animation using an Image UI object in this scene canvas.
                _firstTimeFader.gameObject.SetActive(true);
                LeanTween.alpha(_firstTimeFader, 0, 2f).setOnComplete(() => { Destroy(_firstTimeFader.gameObject); });
            }
        }

        private void Start()
        {
            GameMaster.Instance.Initialize();
            _version.text = $"v{Application.version}";
        }

        // Using LeanTween plugin to do some visual tweening in camera wrapper and title to make this flying movement
        // animation. Looks bulky but its pretty straigh forward
        public void GotoGame()
        {
            if (_tweening) return;

            _tweening = true;

            GameUtils.SetVolume(GetComponent<AudioSource>(), 0, GameMaster.Instance.Fader.FadeInOutDuration.y);

            LeanTween.move(_title, _title.transform.localPosition + Vector3.up * 150, GameMaster.Instance.Fader.FadeInOutDuration.y).setEase(LeanTweenType.easeInQuad);
            LeanTween.scale(_title, _title.transform.localScale * 1.25f, GameMaster.Instance.Fader.FadeInOutDuration.y).setEase(LeanTweenType.easeInQuad);

            LeanTween.moveLocal(_camera, _camera.transform.position + Vector3.up * 0.5f, GameMaster.Instance.Fader.FadeInOutDuration.y).setEase(LeanTweenType.easeInQuad);
            LeanTween.rotateLocal(_camera, _camera.transform.localEulerAngles + Vector3.right * 15, GameMaster.Instance.Fader.FadeInOutDuration.y).setEase(LeanTweenType.easeInQuad);
            
            GameMaster.Instance.LoadScene(1);
        }

        private void Update()
        {
            if (Input.GetKey(KeyCode.Escape))
            {
                GameMaster.Instance.Fader.FadeIn(() =>
                {
                    Debug.Log("Quit App");
                    Application.Quit();
                });
            }
        }
    }
}
