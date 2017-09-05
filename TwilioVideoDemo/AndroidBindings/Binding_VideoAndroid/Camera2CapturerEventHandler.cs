using System;
using Com.Twilio.Video;

namespace Binding_VideoAndroid
{
    public class Camera2CapturerEventHandler
    {
        private readonly Camera2Capturer.IListenerImplementor _listenerImplementor;

        public Camera2Capturer.IListener ListenerImplementor => _listenerImplementor;

        public Camera2CapturerEventHandler(object sender)
        {
            _listenerImplementor = new Camera2Capturer.IListenerImplementor(sender);
        }

        public event EventHandler<Camera2Capturer.CameraSwitchedEventArgs> OnCameraSwitched
        {
            add => _listenerImplementor.OnCameraSwitchedHandler += value;
            remove => _listenerImplementor.OnCameraSwitchedHandler -= value;
        }

        public event EventHandler<Camera2Capturer.ErrorEventArgs> OnError
        {
            add => _listenerImplementor.OnErrorHandler += value;
            remove => _listenerImplementor.OnErrorHandler -= value;
        }

        public event EventHandler OnFirstFrameAvailable
        {
            add => _listenerImplementor.OnFirstFrameAvailableHandler += value;
            remove => _listenerImplementor.OnFirstFrameAvailableHandler -= value;
        }
    }

}