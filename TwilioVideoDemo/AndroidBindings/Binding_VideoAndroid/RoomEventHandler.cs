using System;
using Com.Twilio.Video;

namespace Binding_VideoAndroid
{
    public class RoomEventHandler
    {
        private readonly Room.IListenerImplementor _listenerImplementor;

        public Room.IListener ListenerImplementor => _listenerImplementor;

        public event EventHandler<Room.DisconnectedEventArgs> OnDisconnectedHandler
        {
            add => _listenerImplementor.OnDisconnectedHandler += value;
            remove => _listenerImplementor.OnDisconnectedHandler -= value;
        }

        public event EventHandler<Room.ConnectFailureEventArgs> OnConnectFailureHandler
        {
            add => _listenerImplementor.OnConnectFailureHandler += value;
            remove => _listenerImplementor.OnConnectFailureHandler -= value;
        }

        public event EventHandler<Room.ParticipantConnectedEventArgs> OnParticipantConnectedHandler
        {
            add => _listenerImplementor.OnParticipantConnectedHandler += value;
            remove => _listenerImplementor.OnParticipantConnectedHandler -= value;
        }

        public event EventHandler<Room.ParticipantDisconnectedEventArgs> OnParticipantDisconnectedHandler
        {
            add => _listenerImplementor.OnParticipantDisconnectedHandler += value;
            remove => _listenerImplementor.OnParticipantDisconnectedHandler -= value;
        }

        public event EventHandler<Room.RecordingStartedEventArgs> OnRecordingStartedHandler
        {
            add => _listenerImplementor.OnRecordingStartedHandler += value;
            remove => _listenerImplementor.OnRecordingStartedHandler -= value;
        }

        public event EventHandler<Room.RecordingStoppedEventArgs> OnRecordingStoppedHandler
        {
            add => _listenerImplementor.OnRecordingStoppedHandler += value;
            remove => _listenerImplementor.OnRecordingStoppedHandler -= value;
        }

        public event EventHandler<Room.ConnectedEventArgs> OnConnectedHandler
        {
            add => _listenerImplementor.OnConnectedHandler += value;
            remove => _listenerImplementor.OnConnectedHandler -= value;
        }

        public RoomEventHandler(object sender)
        {
            _listenerImplementor = new Room.IListenerImplementor(sender);
        }
    }

}