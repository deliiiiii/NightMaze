using UnityEngine;

namespace MemoFramework
{
    public class DefaultLogger: ILogger
    {
        public void Log(MFLogLevel level, object message)
        {
            switch (level)
            {
                case MFLogLevel.Debug:
                    Debug.Log(MFUtils.Text.Format("<color=#FF00BE><b>Debug:</b></color>{0}", message.ToString()));
                    break;

                case MFLogLevel.Info:
                    Debug.Log(MFUtils.Text.Format("<b>Info:</b>{0}", message.ToString()));
                    break;

                case MFLogLevel.Warning:
                    Debug.LogWarning(MFUtils.Text.Format("<b><color=#FFA800>Warning:</color></b>{0}", message.ToString()));
                    break;

                case MFLogLevel.Error:
                    Debug.LogError(MFUtils.Text.Format("<b><color=#B40500>Error:</color></b>{0}", message.ToString()));
                    break;

                default:
                    throw new MFException(message.ToString());
            }
        }
    }
}