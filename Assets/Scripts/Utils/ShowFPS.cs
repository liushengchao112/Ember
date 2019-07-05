using UnityEngine;
using System.Collections;

using Utils;
using Constants;

public class ShowFPS : MonoBehaviour
{

    public const float updateInterval = 0.5F;

    private static float lastInterval;

    private static float fps;
    private Rect fpsLabelShow;
    private static int frames = 0;

    private static float nps1;
    private static float nps2;
    private Rect npsLabelShow;
    private static float lastNpsUpdateTime;

    private static float delay;
    private Rect delayLabelShow;

    void Start()
    {
        lastInterval = Time.realtimeSinceStartup;

        frames = 0;
        fps = 0;
		fpsLabelShow = new Rect( Screen.width - 300, 10, 200, 200 );

        lastNpsUpdateTime = 0;
        nps1 = 0;
        nps2 = 0;
        npsLabelShow = new Rect( Screen.width - 300, 25, 200, 200 );

        delay = 0;
        delayLabelShow = new Rect( Screen.width - 300, 40, 200, 200 );
    }

    void OnGUI()
    {
		if (fps < 20.0f)
			GUI.color = Color.red;
		else
			GUI.color = Color.green;
		GUI.Label (fpsLabelShow, fps.ToString( "f2" ) );

        if( nps1 < GameConstants.LOGIC_FRAME_TIME + 0.030f )
            GUI.color = Color.green;
        else
            GUI.color = Color.red;
        GUI.Label( npsLabelShow, nps1.ToString() );

        if( delay < GameConstants.LOGIC_FRAME_TIME + 0.030f )
            GUI.color = Color.green;
        else
            GUI.color = Color.red;
        GUI.Label( delayLabelShow, delay.ToString() );
    }

    void Update()
    {
        ++frames;

        if (Time.realtimeSinceStartup > lastInterval + updateInterval)
        {
            fps = frames / (Time.realtimeSinceStartup - lastInterval);
            frames = 0;

            nps1 = nps2;
            nps2 = 0;

            lastInterval = Time.realtimeSinceStartup;
        }
    }

    public static void UpdateNps()
    {
        if( lastNpsUpdateTime == 0 )
        {
            lastNpsUpdateTime = Time.realtimeSinceStartup;
        }
        else
        {
            float time = Time.realtimeSinceStartup - lastNpsUpdateTime;
            lastNpsUpdateTime = Time.realtimeSinceStartup;

            if( nps2 < time )
            {
                nps2 = time;
            }

            if( delay < time )
            {
                delay = time;
            }
        }
    }
}
