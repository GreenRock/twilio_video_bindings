using Android.Content;
using Android.Util;
using Binding_VideoAndroid;
using Org.Webrtc;

using Camera2Capturer = Com.Twilio.Video.Camera2Capturer;
using CameraCapturer = Com.Twilio.Video.CameraCapturer;
using IVideoCapturer = Com.Twilio.Video.IVideoCapturer;

namespace TwilioVideoDemo.util
{
    public class CameraCapturerCompat
    {
        private static string TAG = "CameraCapturerCompat";

        private readonly CameraCapturer _camera1Capturer;
        private readonly Camera2Capturer _camera2Capturer;
        private (CameraCapturer.CameraSource, string) _frontCameraPair;
        private (CameraCapturer.CameraSource, string) _backCameraPair;

        public CameraCapturerCompat(Context context, CameraCapturer.CameraSource cameraSource)
        {
            var camera2CapturerEventHandler = new Camera2CapturerEventHandler(this);

            camera2CapturerEventHandler.OnCameraSwitched += (sender, newCameraId) =>
            {
                Log.Info(TAG, "onCameraSwitched: newCameraId = " + newCameraId);
            };

            camera2CapturerEventHandler.OnError += (sender, args) =>
            {
                Log.Error(TAG, args.P0.ToString());
            };

            camera2CapturerEventHandler.OnFirstFrameAvailable += (sender, args) =>
            {
                Log.Info(TAG, "onFirstFrameAvailable");
            };

            if (Camera2Capturer.IsSupported(context))
            {
                SetCameraPairs(context);
                _camera2Capturer = new Camera2Capturer(context, GetCameraId(cameraSource), camera2CapturerEventHandler.ListenerImplementor);
            }
            else
            {
                _camera1Capturer = new CameraCapturer(context, cameraSource);
            }
        }

        public CameraCapturer.CameraSource GetCameraSource()
        {
            if (UsingCamera1())
            {
                return _camera1Capturer.GetCameraSource();
            }
            return GetCameraSource(_camera2Capturer.CameraId);
        }

        public void SwitchCamera()
        {
            if (UsingCamera1())
            {
                _camera1Capturer.SwitchCamera();
            }
            else
            {
                CameraCapturer.CameraSource cameraSource = GetCameraSource(_camera2Capturer.CameraId);

                if (cameraSource == CameraCapturer.CameraSource.FrontCamera)
                {
                    _camera2Capturer.SwitchCamera(_backCameraPair.Item2);
                }
                else
                {
                    _camera2Capturer.SwitchCamera(_frontCameraPair.Item2);
                }
            }
        }

        /*
        * This method is required because this class is not an implementation of VideoCapturer due to
        * a shortcoming in the Video Android SDK.
        */
        public IVideoCapturer GetVideoCapturer()
        {
            if (UsingCamera1())
            {
                return _camera1Capturer;
            }
            return _camera2Capturer;
        }

        private bool UsingCamera1()
        {
            return _camera1Capturer != null;
        }

        private void SetCameraPairs(Context context)
        {
            Camera2Enumerator camera2Enumerator = new Camera2Enumerator(context);
            foreach (string cameraId in camera2Enumerator.GetDeviceNames())
            {
                if (camera2Enumerator.IsFrontFacing(cameraId))
                {
                    _frontCameraPair = (CameraCapturer.CameraSource.FrontCamera, cameraId);
                }
                if (camera2Enumerator.IsBackFacing(cameraId))
                {
                    _backCameraPair = (CameraCapturer.CameraSource.BackCamera, cameraId);
                }
            }
        }

        private string GetCameraId(CameraCapturer.CameraSource cameraSource)
        {
            if (_frontCameraPair.Item1 == cameraSource)
            {
                return _frontCameraPair.Item2;
            }
            return _backCameraPair.Item2;
        }

        private CameraCapturer.CameraSource GetCameraSource(string cameraId)
        {
            if (_frontCameraPair.Item2.Equals(cameraId))
            {
                return _frontCameraPair.Item1;
            }
            return _backCameraPair.Item1;
        }
    }
}