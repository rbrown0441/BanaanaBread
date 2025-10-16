using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TunnelPortal : MonoBehaviour
{
    [Header("ID & Target")]
    public string portalId;
    public string targetScene;
    public string targetPortalId;

    [Header("Spawn point (child)")]
    public Transform spawnPoint;

    [Header("UX")]
    [SerializeField] Canvas promptCanvas;

    static readonly Dictionary<string, TunnelPortal> s_registry = new Dictionary<string, TunnelPortal>();
    static string s_pendingScene;
    static string s_pendingPortalId;

    // subscribe count so we only hook sceneLoaded once
    static int s_subscribers = 0;

    void Awake()
    {
        if (!promptCanvas) promptCanvas = GetComponentInChildren<Canvas>(true);
        if (promptCanvas) promptCanvas.enabled = false;
    }

    void OnEnable()
    {
        // registry
        if (!string.IsNullOrEmpty(portalId))
            s_registry[portalId] = this;

        // hook sceneLoaded once across all portals
        if (s_subscribers++ == 0)
            SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        // un-register only if we’re the current mapping
        if (!string.IsNullOrEmpty(portalId) &&
            s_registry.TryGetValue(portalId, out var me) && me == this)
            s_registry.Remove(portalId);

        if (--s_subscribers == 0)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Hook this from InteractiveObject.OnInteract
    public void TravelNow()
    {
        if (string.IsNullOrEmpty(targetScene) || string.IsNullOrEmpty(targetPortalId))
            return;

        // Same-scene hop (no reload)
        if (SceneManager.GetActiveScene().name == targetScene)
        {
            if (TryGetPortal(targetPortalId, out var dest))
                PlacePlayerAt(dest);
            return;
        }

        // Cross-scene: remember target and load
        s_pendingScene = targetScene;
        s_pendingPortalId = targetPortalId;
        SceneManager.LoadScene(targetScene, LoadSceneMode.Single);
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode _)
    {
        if (string.IsNullOrEmpty(s_pendingScene) || scene.name != s_pendingScene)
            return;

        if (TryGetPortal(s_pendingPortalId, out var dest))
        {
            dest.StartCoroutine(dest.CoPlacePlayerWhenReady());
        }
        else
        {
            // clear pending to avoid sticking
            s_pendingScene = null;
            s_pendingPortalId = null;
        }
    }

    IEnumerator CoPlacePlayerWhenReady()
    {
        // wait up to ~2 seconds for Player to exist
        const int maxFrames = 120;
        for (int f = 0; f < maxFrames; f++)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player)
            {
                PlacePlayerAt(this);
                break;
            }
            yield return null;
        }

        // clear pending after attempt
        s_pendingScene = null;
        s_pendingPortalId = null;
    }

    public void ShowPrompt(bool show)
    {
        if (promptCanvas) promptCanvas.enabled = show;
    }

    static bool TryGetPortal(string id, out TunnelPortal p)
    {
        if (!string.IsNullOrEmpty(id) && s_registry.TryGetValue(id, out p))
            return true;

        foreach (var portal in FindObjectsOfType<TunnelPortal>(true))
        {
            if (portal.portalId == id)
            {
                s_registry[id] = portal;
                p = portal;
                return true;
            }
        }
        p = null;
        return false;
    }

    static void PlacePlayerAt(TunnelPortal dest)
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (!player) return;

        var t = dest.spawnPoint ? dest.spawnPoint : dest.transform;
        player.transform.position = t.position;
    }
}
