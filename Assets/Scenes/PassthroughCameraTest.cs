using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/// <summary>
/// Minimaal testscript om te verifiëren:
/// 1. Of de Passthrough Camera API data levert via CPU image capture (frameReceived).
/// 2. Of het AR Camera Background-component visueel passthrough toont aan de gebruiker
///    (dit willen we juist NIET, want de speler moet volledig in VR blijven).
///
/// Zet dit script op een leeg GameObject in je testscene, en sleep de AR Camera Manager
/// erin via de Inspector.
/// </summary>
public class PassthroughCameraTest : MonoBehaviour
{
    [SerializeField] ARCameraManager m_CameraManager;
    [SerializeField] ARCameraBackground m_CameraBackground;

    [Tooltip("Zet dit UIT om te testen of CPU image capture werkt ZONDER dat de gebruiker" +
             "de passthrough-achtergrond ziet. Dit is de kernvraag van deze test.")]
    [SerializeField] bool m_EnableVisualBackground = false;

    int m_FrameCount = 0;

    void OnEnable()
    {
        if (m_CameraManager == null)
        {
            Debug.LogError("[PassthroughTest] Geen ARCameraManager toegewezen!");
            return;
        }

        m_CameraManager.frameReceived += OnFrameReceived;

        // Test: kan de visuele achtergrond-compositing uitgeschakeld blijven
        // terwijl de camera-data callback (frameReceived) alsnog werkt?
        if (m_CameraBackground != null)
        {
            m_CameraBackground.enabled = m_EnableVisualBackground;
            Debug.Log($"[PassthroughTest] AR Camera Background component enabled = {m_EnableVisualBackground}");
        }
    }

    void OnDisable()
    {
        if (m_CameraManager != null)
            m_CameraManager.frameReceived -= OnFrameReceived;
    }

    void OnFrameReceived(ARCameraFrameEventArgs eventArgs)
    {
        m_FrameCount++;

        // Log slechts elke 60 frames (~1x per seconde bij 60Hz) om de log niet te overspoelen.
        if (m_FrameCount % 60 == 0)
        {
            Debug.Log($"[PassthroughTest] Frame ontvangen (#{m_FrameCount}). " +
                      $"CPU image capture werkt dus, ongeacht of de visuele achtergrond aanstaat.");
        }

        // Probeer daadwerkelijk een CPU-image op te halen, om te bevestigen dat de
        // onderliggende data ook echt beschikbaar is (niet alleen het event zelf).
        if (m_CameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
        {
            if (m_FrameCount % 60 == 0)
            {
                Debug.Log($"[PassthroughTest] CPU image succesvol opgehaald: " +
                          $"{image.width}x{image.height}, format: {image.format}");
            }
            image.Dispose(); // Belangrijk: altijd disposen om geheugenlekken te voorkomen.
        }
        else
        {
            if (m_FrameCount % 60 == 0)
                Debug.LogWarning("[PassthroughTest] Kon geen CPU image ophalen (nog).");
        }
    }
}