using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Com.Twilio.Video;

namespace Binding_VideoAndroid
{
    public class ParticipantEventHandler
    {
        private readonly Participant.IListenerImplementor _listenerImplementor;

        public Participant.IListener ListenerImplementor => _listenerImplementor;

        public ParticipantEventHandler(object sender)
        {
            _listenerImplementor = new Participant.IListenerImplementor(sender);
        }

        public event EventHandler<Participant.AudioTrackAddedEventArgs> OnAudioTrackAddedHandler
        {
            add => _listenerImplementor.OnAudioTrackAddedHandler += value;
            remove => _listenerImplementor.OnAudioTrackAddedHandler -= value;
        }

        public event EventHandler<Participant.AudioTrackRemovedEventArgs> OnAudioTrackRemovedHandler
        {
            add => _listenerImplementor.OnAudioTrackRemovedHandler += value;
            remove => _listenerImplementor.OnAudioTrackRemovedHandler -= value;
        }

        public event EventHandler<Participant.VideoTrackAddedEventArgs> OnVideoTrackAddedHandler
        {
            add => _listenerImplementor.OnVideoTrackAddedHandler += value;
            remove => _listenerImplementor.OnVideoTrackAddedHandler -= value;
        }

        public event EventHandler<Participant.VideoTrackRemovedEventArgs> OnVideoTrackRemovedHandler
        {
            add => _listenerImplementor.OnVideoTrackRemovedHandler += value;
            remove => _listenerImplementor.OnVideoTrackRemovedHandler -= value;
        }

        public event EventHandler<Participant.AudioTrackEnabledEventArgs> OnAudioTrackEnabledHandler
        {
            add => _listenerImplementor.OnAudioTrackEnabledHandler += value;
            remove => _listenerImplementor.OnAudioTrackEnabledHandler -= value;
        }

        public event EventHandler<Participant.AudioTrackDisabledEventArgs> OnAudioTrackDisabledHandler
        {
            add => _listenerImplementor.OnAudioTrackDisabledHandler += value;
            remove => _listenerImplementor.OnAudioTrackDisabledHandler -= value;
        }

        public event EventHandler<Participant.VideoTrackEnabledEventArgs> OnVideoTrackEnabledHandler
        {
            add => _listenerImplementor.OnVideoTrackEnabledHandler += value;
            remove => _listenerImplementor.OnVideoTrackEnabledHandler -= value;
        }

        public event EventHandler<Participant.VideoTrackDisabledEventArgs> OnVideoTrackDisabledHandler
        {
            add => _listenerImplementor.OnVideoTrackDisabledHandler += value;
            remove => _listenerImplementor.OnVideoTrackDisabledHandler -= value;
        }
    }

}