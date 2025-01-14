
using UnityEngine;
using UnityEngine.Rendering;

public class SoundFXManager : MonoBehaviour
{
    // Start is called before the first frame update
    public static SoundFXManager Instance;

    [SerializeField] AudioSource soundFXobject;

    /*
     [SerializeField] AudioClip FearHit;
    [SerializeField] AudioClip FireHit;
    [SerializeField] AudioClip NatureHit;
    [SerializeField] AudioClip MetalonFlesh;
    [SerializeField] AudioClip MetalonLeather;
    [SerializeField] AudioClip MetalonMetal;
    [SerializeField] AudioClip WoodonFlesh;
    [SerializeField] AudioClip WoodonLeather;
    [SerializeField] AudioClip WoodonMetal;
    [SerializeField] AudioClip markOfDeathCast;
    [SerializeField] AudioClip soulInFusionCast;
    [SerializeField] AudioClip massReanimationCast;
    [SerializeField] AudioClip MouseOver;
    [SerializeField] AudioClip Select;
    [SerializeField] AudioClip Change;
    */
    float defaultVolume =1;
    //[SerializeField] AudioClip ;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }


    public void playSFXClip(AudioClip audioClip, Transform spawnTransform, float volume) //the way the new sound effect is created 
    {
        AudioSource source = Instantiate(soundFXobject, spawnTransform.position, Quaternion.identity);  //spwn a new prefab to play the cllip and die
        source.clip = audioClip;
        source.volume = volume;
        source.Play();
        float audioClipLength = source.clip.length;
        Destroy(source.gameObject, audioClipLength);


    }

    /*
    public void hitSound(string weapon, string armor, Transform spawnTransform, float volume) //checks for the attackers weapon and defenders armour material and using the propriate sound
    {
        switch (weapon)
        {
            // cases where the armor does not matter
            case "Fear":
                playSFXClip(FearHit, spawnTransform, volume);
                break;
            case "Fire":
                playSFXClip(FireHit, spawnTransform, volume);
                break;
            case "Nature":
                playSFXClip(NatureHit, spawnTransform, volume);
                break;



            case "Metal":
                switch (armor)
                {
                    case "Flesh":
                        playSFXClip(MetalonFlesh, spawnTransform, volume);
                        break;
                    case "Leather":
                        playSFXClip(MetalonLeather, spawnTransform, volume);
                        break;
                    case "Metal":
                        playSFXClip(MetalonMetal, spawnTransform, volume);
                        break;
                }
                break;
            case "Wood":
                switch (armor)
                {
                    case "Flesh":
                        playSFXClip(WoodonFlesh, spawnTransform, volume);
                        break;
                    case "Leather":
                        playSFXClip(WoodonLeather, spawnTransform, volume);
                        break;
                    case "Metal":
                        playSFXClip(WoodonMetal, spawnTransform, volume);
                        break;
                }
                break;

        }
    }

    public void playMarkOfDeathCast(Transform spawnTransform, float volume)
    {
        playSFXClip(markOfDeathCast, spawnTransform, volume);
    }

    public void playSoulInFusionCast(Transform spawnTransform, float volume)
    {
        playSFXClip(soulInFusionCast, spawnTransform, volume);
    }

    public void playMassReanimationCast(Transform spawnTransform, float volume)
    {
        playSFXClip(massReanimationCast, spawnTransform, volume);
    }

    public void playMouseOver()
    {
        playSFXClip(MouseOver, transform, defaultVolume);
    }

    public void playSelect()
    {
        playSFXClip(Select, transform, defaultVolume);
    }

    public void playChange()
    {
        playSFXClip(Change, transform, defaultVolume);
    }
    */
}

