using System;
using System.Runtime.InteropServices;
using System.Text;

namespace EasyTool.SystemCategory
{
    /// <summary>
    /// 音频设备控制工具类
    /// </summary>
    public static class AudioUtil
    {
        #region 音量控制

        /// <summary>
        /// 获取主音量（0-100）
        /// </summary>
        public static int GetVolume()
        {
            try
            {
                var enumerator = (IMMDeviceEnumerator)new MMDeviceEnumerator();
                var device = enumerator.GetDefaultAudioEndpoint(0, 0); // ERender = 0, eConsole = 0
                var iid = typeof(IAudioEndpointVolume).GUID;
                device.Activate(ref iid, 0, IntPtr.Zero, out var interfacePtr);
                var audioEndpoint = (IAudioEndpointVolume)Marshal.GetObjectForIUnknown(interfacePtr);
                var volume = audioEndpoint.GetMasterVolumeLevelScalar();
                Marshal.Release(interfacePtr);
                return (int)(volume * 100);
            }
            catch
            {
                return -1;
            }
        }

        /// <summary>
        /// 设置主音量（0-100）
        /// </summary>
        public static bool SetVolume(int volume)
        {
            try
            {
                if (volume < 0 || volume > 100)
                    return false;

                var enumerator = (IMMDeviceEnumerator)new MMDeviceEnumerator();
                var device = enumerator.GetDefaultAudioEndpoint(0, 0);
                var iid = typeof(IAudioEndpointVolume).GUID;
                device.Activate(ref iid, 0, IntPtr.Zero, out var interfacePtr);
                var audioEndpoint = (IAudioEndpointVolume)Marshal.GetObjectForIUnknown(interfacePtr);
                audioEndpoint.SetMasterVolumeLevelScalar(volume / 100f, Guid.Empty);
                Marshal.Release(interfacePtr);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 是否静音
        /// </summary>
        public static bool IsMuted()
        {
            try
            {
                var enumerator = (IMMDeviceEnumerator)new MMDeviceEnumerator();
                var device = enumerator.GetDefaultAudioEndpoint(0, 0);
                var iid = typeof(IAudioEndpointVolume).GUID;
                device.Activate(ref iid, 0, IntPtr.Zero, out var interfacePtr);
                var audioEndpoint = (IAudioEndpointVolume)Marshal.GetObjectForIUnknown(interfacePtr);
                var result = audioEndpoint.GetMute() != 0;
                Marshal.Release(interfacePtr);
                return result;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 设置静音
        /// </summary>
        public static bool SetMute(bool mute)
        {
            try
            {
                var enumerator = (IMMDeviceEnumerator)new MMDeviceEnumerator();
                var device = enumerator.GetDefaultAudioEndpoint(0, 0);
                var iid = typeof(IAudioEndpointVolume).GUID;
                device.Activate(ref iid, 0, IntPtr.Zero, out var interfacePtr);
                var audioEndpoint = (IAudioEndpointVolume)Marshal.GetObjectForIUnknown(interfacePtr);
                audioEndpoint.SetMute(mute ? 1 : 0, Guid.Empty);
                Marshal.Release(interfacePtr);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 切换静音状态
        /// </summary>
        public static bool ToggleMute()
        {
            return SetMute(!IsMuted());
        }

        /// <summary>
        /// 音量增加
        /// </summary>
        public static bool VolumeUp(int amount = 5)
        {
            var current = GetVolume();
            if (current < 0) return false;
            return SetVolume(Math.Min(100, current + amount));
        }

        /// <summary>
        /// 音量减少
        /// </summary>
        public static bool VolumeDown(int amount = 5)
        {
            var current = GetVolume();
            if (current < 0) return false;
            return SetVolume(Math.Max(0, current - amount));
        }

        #endregion

        #region COM接口

        [ComImport, Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
        private class MMDeviceEnumerator { }

        [ComImport, Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IMMDeviceEnumerator
        {
            int EnumAudioEndpoints(int dataFlow, int stateMask, out IntPtr devices);
            IMMDevice GetDefaultAudioEndpoint(int dataFlow, int role);
            int GetDevice(string id, out IntPtr device);
            int RegisterEndpointNotificationCallback(IntPtr client);
            int UnregisterEndpointNotificationCallback(IntPtr client);
        }

        [ComImport, Guid("D666063F-1587-4E43-81F1-B948E807363F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IMMDevice
        {
            int Activate(ref Guid iid, int dwClsCtx, IntPtr activationParams, out IntPtr interfacePtr);
            int OpenPropertyStore(int stgmAccess, out IntPtr properties);
            int GetId(out string id);
            int GetState(out int state);
        }

        [ComImport, Guid("5CDF2C82-841E-4546-9722-0CF74078229A"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IAudioEndpointVolume
        {
            int RegisterControlChangeNotify(IntPtr notify);
            int UnregisterControlChangeNotify(IntPtr notify);
            int GetChannelCount(out int channelCount);
            int SetMasterVolumeLevel(float level, Guid eventContext);
            int SetMasterVolumeLevelScalar(float level, Guid eventContext);
            float GetMasterVolumeLevel();
            float GetMasterVolumeLevelScalar();
            int SetChannelVolumeLevel(int channel, float level, Guid eventContext);
            int SetChannelVolumeLevelScalar(int channel, float level, Guid eventContext);
            float GetChannelVolumeLevel(int channel);
            float GetChannelVolumeLevelScalar(int channel);
            int SetMute(int mute, Guid eventContext);
            int GetMute();
        }

        #endregion
    }

    /// <summary>
    /// 系统提示音
    /// </summary>
    public static class SystemSoundUtil
    {
        /// <summary>
        /// 播放系统提示音
        /// </summary>
        public static void Beep()
        {
            Console.Beep();
        }

        /// <summary>
        /// 播放指定频率和时长的提示音
        /// </summary>
        public static void Beep(int frequency, int duration)
        {
            Console.Beep(frequency, duration);
        }

        /// <summary>
        /// 播放系统默认声音
        /// </summary>
        public static void PlayDefault()
        {
            MessageBeep(0xFFFFFFFF);
        }

        /// <summary>
        /// 播放系统错误声音
        /// </summary>
        public static void PlayError()
        {
            MessageBeep(0x10);
        }

        /// <summary>
        /// 播放系统问号声音
        /// </summary>
        public static void PlayQuestion()
        {
            MessageBeep(0x20);
        }

        /// <summary>
        /// 播放系统警告声音
        /// </summary>
        public static void PlayWarning()
        {
            MessageBeep(0x30);
        }

        /// <summary>
        /// 播放系统信息声音
        /// </summary>
        public static void PlayInformation()
        {
            MessageBeep(0x40);
        }

        [DllImport("user32.dll")]
        private static extern bool MessageBeep(uint type);
    }
}
