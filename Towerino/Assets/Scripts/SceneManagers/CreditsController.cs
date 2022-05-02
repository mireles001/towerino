using UnityEngine;

namespace Towerino
{
    // Credits scene controller... Just starts and rotate a prop and that it.
    public class CreditsController : MonoBehaviour
    {
        [SerializeField]
        private Transform _squire = null;
        [SerializeField]
        private float _spinSpeed = 10;

        private bool _tweening;

        private void Awake()
        {
            GameMaster.Instance.Fader.FadeOut();
        }

        private void Start()
        {
            GameMaster.Instance.Initialize();
        }

        private void Update()
        {
            _squire.Rotate(Vector3.up * _spinSpeed * Time.deltaTime);

            if (Input.GetKey(KeyCode.Escape)) GotoMenu();
        }

        private void GotoMenu()
        {
            if (_tweening) return;

            _tweening = true;

            GameUtils.SetVolume(GetComponent<AudioSource>(), 0, GameMaster.Instance.Fader.FadeInOutDuration.y);
            GameMaster.Instance.LoadScene(0);
        }
    }
}

