using System.Linq;
using UnityEngine;

namespace DonutStack.Common.Audio
{
    public class AudioController : MonoBehaviour
    {
        public static AudioController Instance;

        [Header("Sources")]
        [SerializeField] AudioSource[] stackedObjectsSources;
        [SerializeField] AudioSource UISource;
        [Header("Clips")]
        [SerializeField] AudioClip popClip;
        [SerializeField] AudioClip fallClip;
        [SerializeField] AudioClip winClip;
        [SerializeField] AudioClip loseClip;


        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        public void PlaySound(Sounds sound) 
        {
            AudioSource source = FindFreeSource(sound);
            AudioClip clip = GetClip(sound);
            if (source == null)
            {
                return;
            }
            if (clip == null)
            {
                return;
            }
            source.clip = clip;
            source.Play();
        }

        private AudioSource FindFreeSource(Sounds sound) 
        {
            switch (sound)
            {
                case Sounds.DonutPop:
                case Sounds.DonutFall:
                    return stackedObjectsSources.FirstOrDefault(s => s.isPlaying == false);
                case Sounds.WinSound:
                case Sounds.LoseSound:
                    return UISource.isPlaying ? null : UISource;
                default:
                    return null;
            }
        }

        private AudioClip GetClip(Sounds sound) 
        {
            switch (sound)
            {
                case Sounds.DonutPop:
                    return popClip;
                case Sounds.DonutFall:
                    return fallClip;
                case Sounds.WinSound:
                    return winClip;
                case Sounds.LoseSound:
                    return loseClip;
                default:
                    return null;
            }
        }


        public enum Sounds : byte
        {
            DonutPop,
            DonutFall,
            WinSound,
            LoseSound
        }

    }
}
