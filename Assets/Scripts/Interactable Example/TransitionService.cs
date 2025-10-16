using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Linq;

public sealed class TransitionService : MonoBehaviour
{
    static TransitionService _inst;
    static string _pendingScene;
    static string _pendingTargetPortalId;
    static int _pendingPhaseDelta;

    // Reset statics between play sessions (covers Domain Reload OFF)
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        _inst = null;
        _pendingScene = null;
        _pendingTargetPortalId = null;
        _pendingPhaseDelta = 0;
    }

    void Awake()
    {
        if (_inst == null)
        {
            _inst = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else if (_inst != this)
        {
            // never destroy the host GameObject; just remove the extra component
            Destroy(this);
        }
    }

    void OnDestroy()
    {
        if (_inst == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            _inst = null;
        }
    }

    public static void Travel(string sceneName, string targetPortalId, int phaseDelta = 0)
    {
        EnsureInstance();
        _pendingScene = sceneName;
        _pendingTargetPortalId = targetPortalId;
        _pendingPhaseDelta = phaseDelta;
        _inst.StartCoroutine(_inst.LoadScene(sceneName));
    }

    static void EnsureInstance()
    {
        if (_inst != null) return;
        var go = new GameObject("TransitionService");
        _inst = go.AddComponent<TransitionService>(); // Awake wires events + DDOL
    }

    IEnumerator LoadScene(string sceneName)
    {
        var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        while (!op.isDone) yield return null; // OnSceneLoaded will fire next
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (string.IsNullOrEmpty(_pendingTargetPortalId) || scene.name != _pendingScene)
            return;

        // find the target portal in the loaded scene
        var target = FindObjectsOfType<TunnelPortal>(true)
                     .FirstOrDefault(p => p.portalId == _pendingTargetPortalId);

        if (target != null)
        {
            // find player
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                var cs = FindObjectOfType<CharacterScript>();
                if (cs != null) player = cs.gameObject;
            }

            // place player at portal (or its spawn point)
            if (player != null)
            {
                var t = target.spawnPoint != null ? target.spawnPoint : target.transform;
                player.transform.position = t.position;
            }

            // apply time phase steps if requested
            for (int i = 0; i < _pendingPhaseDelta; i++)
                StepTime.Advance();
        }

        // clear pending state
        _pendingScene = null;
        _pendingTargetPortalId = null;
        _pendingPhaseDelta = 0;
    }
}
