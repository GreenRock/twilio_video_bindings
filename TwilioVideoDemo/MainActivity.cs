using System.Collections.Generic;
using System.Linq;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Media;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;
using Com.Twilio.Video;
using Org.Webrtc;
using TwilioVideoDemo.util;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Util;
using Android.Views;
using Binding_VideoAndroid;
using Java.Lang;
using Java.Util;
using CameraCapturer = Com.Twilio.Video.CameraCapturer;
using IVideoRenderer = Com.Twilio.Video.IVideoRenderer;
using VideoTrack = Com.Twilio.Video.VideoTrack;
using VideoView = Com.Twilio.Video.VideoView;

namespace TwilioVideoDemo
{
    [Activity(Label = "TwilioVideoDemo", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private static int CAMERA_MIC_PERMISSION_REQUEST_CODE = 1;
        private static string TAG = "VideoActivity";
        private static string TWILIO_ACCESS_TOKEN = "TWILIO_ACCESS_TOKEN";
        /*
 * Access token used to connect. This field will be set either from the console generated token
 * or the request to the token server.
 */
        private string accessToken;

        /*
         * A Room represents communication between a local participant and one or more participants.
         */
        private Room room;
        private LocalParticipant localParticipant;

        /*
         * A VideoView receives frames from a local or remote video track and renders them
         * to an associated view.
         */
        private VideoView primaryVideoView;
        private VideoView thumbnailVideoView;

        /*
     * Android application UI elements
     */
        private TextView videoStatusTextView;
        private CameraCapturerCompat cameraCapturerCompat;
        private LocalAudioTrack localAudioTrack;
        private LocalVideoTrack localVideoTrack;
        private FloatingActionButton connectActionFab;
        private FloatingActionButton switchCameraActionFab;
        private FloatingActionButton localVideoActionFab;
        private FloatingActionButton muteActionFab;
        private AlertDialog alertDialog;
        private AudioManager audioManager;
        private string participantIdentity;

        private Mode previousAudioMode;
        private bool previousMicrophoneMute;
        private IVideoRenderer localVideoView;
        private bool disconnectedFromOnDestroy;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_video);

            primaryVideoView = (VideoView)FindViewById(Resource.Id.primary_video_view);
            thumbnailVideoView = (VideoView)FindViewById(Resource.Id.thumbnail_video_view);
            videoStatusTextView = (TextView)FindViewById(Resource.Id.video_status_textview);

            connectActionFab = (FloatingActionButton)FindViewById(Resource.Id.connect_action_fab);
            switchCameraActionFab = (FloatingActionButton)FindViewById(Resource.Id.switch_camera_action_fab);
            localVideoActionFab = (FloatingActionButton)FindViewById(Resource.Id.local_video_action_fab);
            muteActionFab = (FloatingActionButton)FindViewById(Resource.Id.mute_action_fab);

            /*
  * Enable changing the volume using the up/down keys during a conversation
  */

            VolumeControlStream = Stream.VoiceCall;

            /*
             * Needed for setting/abandoning audio focus during call
             */
            audioManager = (AudioManager)GetSystemService(Context.AudioService);

            /*
             * Check camera and microphone permissions. Needed in Android M.
             */
            if (!CheckPermissionForCameraAndMicrophone())
            {
                RequestPermissionForCameraAndMicrophone();
            }
            else
            {
                CreateAudioAndVideoTracks();
                SetAccessToken();
            }

            /*
             * Set the initial state of the UI
             */
            IntializeUi();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            if (requestCode == CAMERA_MIC_PERMISSION_REQUEST_CODE)
            {
                bool cameraAndMicPermissionGranted = true;

                foreach (var grantResult in grantResults)
                {
                    cameraAndMicPermissionGranted &= grantResult == Permission.Granted;
                }

                if (cameraAndMicPermissionGranted)
                {
                    CreateAudioAndVideoTracks();
                    SetAccessToken();
                }
                else
                {
                    Toast.MakeText(this, Resource.String.permissions_needed, ToastLength.Long).Show();
                }
            }
        }

        protected override void OnResume()
        {
            base.OnResume();

            if (localVideoTrack == null && CheckPermissionForCameraAndMicrophone())
            {
                localVideoTrack = LocalVideoTrack.Create(this, true, cameraCapturerCompat.GetVideoCapturer());
                localVideoTrack.AddRenderer(localVideoView);

                /*
                 * If connected to a Room then share the local video track.
                 */
                if (localParticipant != null)
                {
                    localParticipant.AddVideoTrack(localVideoTrack);
                }
            }
        }

        protected override void OnPause()
        {
            if (localVideoTrack != null)
            {
                /*
                 * If this local video track is being shared in a Room, remove from local
                 * participant before releasing the video track. Participants will be notified that
                 * the track has been removed.
                 */
                if (localParticipant != null)
                {
                    localParticipant.RemoveVideoTrack(localVideoTrack);
                }

                localVideoTrack.Release();
                localVideoTrack = null;
            }
            base.OnPause();
        }

        protected override void OnDestroy()
        {
            if (room != null && room.State != RoomState.Disconnected)
            {
                room.Disconnect();
                disconnectedFromOnDestroy = true;
            }

            /*
             * Release the local audio and video tracks ensuring any memory allocated to audio
             * or video is freed.
             */
            if (localAudioTrack != null)
            {
                localAudioTrack.Release();
                localAudioTrack = null;
            }
            if (localVideoTrack != null)
            {
                localVideoTrack.Release();
                localVideoTrack = null;
            }

            base.OnDestroy();
        }

        private bool CheckPermissionForCameraAndMicrophone()
        {
            Permission resultCamera = ContextCompat.CheckSelfPermission(this, Manifest.Permission.Camera);
            Permission resultMic = ContextCompat.CheckSelfPermission(this, Manifest.Permission.RecordAudio);
            return resultCamera == Permission.Granted && resultMic == Permission.Granted;
        }

        private void RequestPermissionForCameraAndMicrophone()
        {
            if (ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.Camera) ||
                ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.RecordAudio))
            {
                Toast.MakeText(this, Resource.String.permissions_needed, ToastLength.Long).Show();
            }
            else
            {
                ActivityCompat.RequestPermissions(
                    this,
                    new[] { Manifest.Permission.Camera, Manifest.Permission.RecordAudio }, CAMERA_MIC_PERMISSION_REQUEST_CODE);
            }
        }

        private void CreateAudioAndVideoTracks()
        {
            // Share your microphone
            localAudioTrack = LocalAudioTrack.Create(this, true);

            // Share your camera
            cameraCapturerCompat = new CameraCapturerCompat(this, CameraCapturer.CameraSource.FrontCamera);
            localVideoTrack = LocalVideoTrack.Create(this, true, cameraCapturerCompat.GetVideoCapturer());
            primaryVideoView.SetMirror(true);
            localVideoTrack.AddRenderer(primaryVideoView);
            localVideoView = primaryVideoView;
        }

        private void SetAccessToken()
        {
            // OPTION 1- Generate an access token from the getting started portal
            // https://www.twilio.com/console/video/dev-tools/testing-tools
            this.accessToken = TWILIO_ACCESS_TOKEN;

            // OPTION 2- Retrieve an access token from your own web app
            // retrieveAccessTokenfromServer();
        }

        private void ConnectToRoom(string roomName)
        {
            ConfigureAudio(true);
            ConnectOptions.Builder connectOptionsBuilder = new ConnectOptions.Builder(accessToken)
                .RoomName(roomName);


            /*
             * Add local audio track to connect options to share with participants.
             */
            if (localAudioTrack != null)
            {
                connectOptionsBuilder
                    .AudioTracks(new List<LocalAudioTrack> { localAudioTrack });
            }

            /*
             * Add local video track to connect options to share with participants.
             */
            if (localVideoTrack != null)
            {
                connectOptionsBuilder.VideoTracks(new List<LocalVideoTrack> { localVideoTrack });
            }
            room = Video.Connect(this, connectOptionsBuilder.Build(), RoomListener()); SetDisconnectAction();
        }

        /*
         * The initial state when there is no active room.
         */
        private void IntializeUi()
        {
            connectActionFab.SetImageDrawable(ContextCompat.GetDrawable(this, Resource.Drawable.ic_call_white_24px));
            connectActionFab.Show();
            //connectActionFab.SetOnClickListener(ConnectActionClickListener());
            connectActionFab.Click += (sender, args) =>
            {
                ShowConnectDialog();
            };

            switchCameraActionFab.Show();
            //switchCameraActionFab.SetOnClickListener(SwitchCameraClickListener());
            switchCameraActionFab.Click += (sender, args) =>
            {
                if (cameraCapturerCompat != null)
                {
                    CameraCapturer.CameraSource cameraSource = cameraCapturerCompat.GetCameraSource();
                    cameraCapturerCompat.SwitchCamera();
                    if (thumbnailVideoView.Visibility == ViewStates.Visible)
                    {
                        thumbnailVideoView.SetMirror(cameraSource == CameraCapturer.CameraSource.BackCamera);
                    }
                    else
                    {
                        primaryVideoView.SetMirror(cameraSource == CameraCapturer.CameraSource.BackCamera);
                    }
                }
            };

            
            localVideoActionFab.Show();
            //localVideoActionFab.SetOnClickListener(LocalVideoClickListener());
            localVideoActionFab.Click += (sender, args) =>
            {
                /*
                 * Enable/disable the local video track
                 */
                if (localVideoTrack != null)
                {
                    bool enable = !localVideoTrack.IsEnabled;
                    localVideoTrack.Enable(enable);
                    int icon;
                    if (enable)
                    {
                        icon = Resource.Drawable.ic_videocam_green_24px;
                        switchCameraActionFab.Show();
                    }
                    else
                    {
                        icon = Resource.Drawable.ic_videocam_off_red_24px;
                        switchCameraActionFab.Hide();
                    }
                    localVideoActionFab.SetImageDrawable(ContextCompat.GetDrawable(this, icon));
                }
           
            };
           
            muteActionFab.Show();
            //muteActionFab.SetOnClickListener(MuteClickListener());
            muteActionFab.Click += (sender, args) =>
            {
                if (localAudioTrack != null)
                {
                    bool enable = !localAudioTrack.IsEnabled;
                    localAudioTrack.Enable(enable);
                    int icon = enable ?
                        Resource.Drawable.ic_mic_green_24px : Resource.Drawable.ic_mic_off_red_24px;
                    muteActionFab.SetImageDrawable(ContextCompat.GetDrawable(this, icon));
                }
            };
        }

        /*
    * The actions performed during disconnect.
    */
        private void SetDisconnectAction()
        {
            connectActionFab.SetImageDrawable(ContextCompat.GetDrawable(this,
                Resource.Drawable.ic_call_end_white_24px));
            connectActionFab.Show();
            //connectActionFab.SetOnClickListener(DisconnectClickListener());
            connectActionFab.Click += (sender, args) =>
            {
                if (room != null)
                {
                    room.Disconnect();
                }
                IntializeUi();
            };
        }

        /*
         * Creates an connect UI dialog
         */
        private void ShowConnectDialog()
        {
            EditText roomEditText = new EditText(this);
            alertDialog = dialog.Dialog.CreateConnectDialog(roomEditText, (sender, args) =>
            {
                ConnectToRoom(roomEditText.Text);
            }, (sender, args) =>
            {
                IntializeUi();
                alertDialog.Dismiss();
            }, this);
            alertDialog.Show();
        }

        /*
         * Called when participant joins the room
         */
        private void AddParticipant(Participant participant)
        {
            /*
             * This app only displays video for one additional participant per Room
             */
            if (thumbnailVideoView.Visibility == ViewStates.Visible)
            {
                Snackbar.Make(connectActionFab,
                        "Multiple participants are not currently support in this UI",
                        Snackbar.LengthLong).SetAction("Action", view => { }).Show();
                return;
            }
            participantIdentity = participant.Identity;
            videoStatusTextView.Text = "Participant " + participantIdentity + " joined";

            /*
             * Add participant renderer
             */
            if (participant.VideoTracks.Any())
            {
                AddParticipantVideo(participant.VideoTracks[0]);
            }

            /*
             * Start listening for participant events
             */
            participant.SetListener(ParticipantListener());
        }

        /*
    * Set primary view as renderer for participant video track
    */
        private void AddParticipantVideo(VideoTrack videoTrack)
        {
            MoveLocalVideoToThumbnailView();
            primaryVideoView.SetMirror(false);
            videoTrack.AddRenderer(primaryVideoView);
        }

        private void MoveLocalVideoToThumbnailView()
        {
            if (thumbnailVideoView.Visibility == ViewStates.Gone)
            {
                thumbnailVideoView.Visibility = ViewStates.Visible;
                localVideoTrack.RemoveRenderer(primaryVideoView);
                localVideoTrack.AddRenderer(thumbnailVideoView);
                localVideoView = thumbnailVideoView;
                thumbnailVideoView.SetMirror(cameraCapturerCompat.GetCameraSource() ==
                                             CameraCapturer.CameraSource.FrontCamera);
            }
        }

        /*
         * Called when participant leaves the room
         */
        private void RemoveParticipant(Participant participant)
        {
            videoStatusTextView.Text = "Participant " + participant.Identity + " left.";
            if (!participant.Identity.Equals(participantIdentity))
            {
                return;
            }

            /*
             * Remove participant renderer
             */
            if (participant.VideoTracks.Any())
            {
                RemoveParticipantVideo(participant.VideoTracks[0]);
            }
            MoveLocalVideoToPrimaryView();
        }

        private void RemoveParticipantVideo(VideoTrack videoTrack)
        {
            videoTrack.RemoveRenderer(primaryVideoView);
        }

        private void MoveLocalVideoToPrimaryView()
        {
            if (thumbnailVideoView.Visibility == ViewStates.Visible)
            {
                localVideoTrack.RemoveRenderer(thumbnailVideoView);
                thumbnailVideoView.Visibility = ViewStates.Gone;
                localVideoTrack.AddRenderer(primaryVideoView);
                localVideoView = primaryVideoView;
                primaryVideoView.SetMirror(cameraCapturerCompat.GetCameraSource() ==
                                           CameraCapturer.CameraSource.FrontCamera);
            }
        }

        /*
     * Room events listener
     */
        private Room.IListener RoomListener()
        {
            var roomEventHandler = new RoomEventHandler(this);
            roomEventHandler.OnConnectedHandler += (sender, args) =>
            {
                var roomArg = args.P0;

                localParticipant = roomArg.LocalParticipant;
                videoStatusTextView.Text = "Connected to " + roomArg.Name;

                Title = roomArg.Name;

                foreach (Participant participant in room.Participants)
                {
                    AddParticipant(participant);
                    break;
                }
            };

            roomEventHandler.OnConnectFailureHandler += (sender, args) =>
            {
                videoStatusTextView.Text = "Failed to connect";
                ConfigureAudio(false);
            };

            roomEventHandler.OnDisconnectedHandler += (sender, args) =>
            {
                var roomArg = args.P0;
                localParticipant = null;
                videoStatusTextView.Text = "Disconnected from " + roomArg.Name;
                room = null;
                // Only reinitialize the UI if disconnect was not called from onDestroy()
                if (!disconnectedFromOnDestroy)
                {
                    ConfigureAudio(false);
                    IntializeUi();
                    MoveLocalVideoToPrimaryView();
                }
            };

            roomEventHandler.OnParticipantConnectedHandler += (sender, args) =>
            {
                AddParticipant(args.P1);
            };

            roomEventHandler.OnParticipantDisconnectedHandler += (sender, args) =>
            {
                RemoveParticipant(args.P1);
            };

            roomEventHandler.OnRecordingStartedHandler += (sender, args) =>
            {
                Log.Debug(TAG, "onRecordingStarted");
            };

            roomEventHandler.OnRecordingStoppedHandler += (sender, args) =>
            {
                Log.Debug(TAG, "onRecordingStopped");
            };

            return roomEventHandler.ListenerImplementor;
        }


        private Participant.IListener ParticipantListener()
        {
            var participantEventHandler = new ParticipantEventHandler(this);

            participantEventHandler.OnAudioTrackAddedHandler += (sender, args) =>
            {
                videoStatusTextView.Text = "onAudioTrackAdded";
            };

            participantEventHandler.OnAudioTrackRemovedHandler += (sender, args) =>
            {
                videoStatusTextView.Text = "onAudioTrackRemoved";
            };

            participantEventHandler.OnVideoTrackAddedHandler += (sender, args) =>
            {
                videoStatusTextView.Text = "onVideoTrackAdded";
                AddParticipantVideo(args.P1);
            };

            participantEventHandler.OnVideoTrackRemovedHandler += (sender, args) =>
            {
                videoStatusTextView.Text = "onVideoTrackRemoved";
                RemoveParticipantVideo(args.P1);
            };

            participantEventHandler.OnAudioTrackEnabledHandler += (sender, args) => { };
            participantEventHandler.OnAudioTrackDisabledHandler += (sender, args) => { };
            participantEventHandler.OnVideoTrackEnabledHandler += (sender, args) => { };
            participantEventHandler.OnVideoTrackDisabledHandler += (sender, args) => { };

            return participantEventHandler.ListenerImplementor;
        }

        private void ConfigureAudio(bool enable)
        {
            if (enable)
            {
                previousAudioMode = audioManager.Mode;
                // Request audio focus before making any device switch.
                audioManager.RequestAudioFocus(null, Stream.VoiceCall,
                    AudioFocus.GainTransient);
                /*
                 * Use MODE_IN_COMMUNICATION as the default audio mode. It is required
                 * to be in this mode when playout and/or recording starts for the best
                 * possible VoIP performance. Some devices have difficulties with
                 * speaker mode if this is not set.
                 */
                audioManager.Mode = Mode.InCommunication;
                /*
                 * Always disable microphone mute during a WebRTC call.
                 */
                previousMicrophoneMute = audioManager.MicrophoneMute;
                audioManager.MicrophoneMute = false;
            }
            else
            {
                audioManager.Mode = previousAudioMode;
                audioManager.AbandonAudioFocus(null);
                audioManager.MicrophoneMute = previousMicrophoneMute;
            }
        }

    }
}

