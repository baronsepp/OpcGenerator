
namespace NodeGenerator
{
    internal static class NodeGenerator
    {
        private static string _endpointUrl = "opc.tcp://127.0.0.1:49320";
        private static string _nsu = "KEPServerEX";
        private static string _channel = "Channel1";
        private static string _device = "Device1";

        public static string EndpointUrl
        {
            get => _endpointUrl;
            set
            {
                if (value != "") _endpointUrl = value;
            }
        }

        public static string Nsu
        {
            get => _nsu;
            set
            {
                if (value != "") _nsu = value;
            }
        }

        public static string Channel
        {
            get => _channel;
            set
            {
                if (value != "") _channel = value;
            }
        }

        public static string Device
        {
            get => _device;
            set
            {
                if (value != "")_device = value;
            }
        }

        public static string Prefix => $"nsu={_nsu};ns={_channel}.{_device}.";
    }
}