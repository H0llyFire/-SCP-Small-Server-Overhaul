using Synapse;
using Synapse.Api;

namespace SCPSLEnforcedRNG
{
    public class DebugTranslator
    {
        private const string prefix = "\n[EnforcedRNG]: ";


        public static void Console(string message, int messageType = 0, bool forced = false)  //Message Types: 0 Info | 1 Warn | 2 Error
        {
            if (!(PluginClass.ServerConfigs.ShowDebugInConsole || forced)) return;
            switch(messageType)
            {
                case 0:
                    Logger.Get.Info(TranslatePrefix(message));
                    break;
                case 1:
                    Logger.Get.Warn(TranslatePrefix(message));
                    break;
                case 2:
                    Logger.Get.Error(TranslatePrefix(message));
                    break;
                default:
                    break;
            }

        }

        public static string TranslatePrefix(string message)
        {
            List<string> messageLines = new();
            while (message.Contains("\n"))
            {
                int index = message.IndexOf('\n');
                messageLines.Add(message.Substring(0, index));
                message = message.Substring(index + 1);
            }
            string outputMessage = "";
            if (messageLines.Count == 0) outputMessage += prefix + message;
            else
            {
                foreach (var line in messageLines)
                {
                    outputMessage += prefix + line;
                }
                outputMessage += prefix + message;
            }

            return outputMessage;
        }
    }
}
