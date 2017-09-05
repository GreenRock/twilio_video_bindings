using System;
using Android.Content;
using Android.Widget;
using AlertDialog = Android.Support.V7.App.AlertDialog;
namespace TwilioVideoDemo.dialog
{
    public static class Dialog
    {
        public static AlertDialog CreateConnectDialog(EditText participantEditText, EventHandler<DialogClickEventArgs> callParticipantsClickListener, EventHandler<DialogClickEventArgs> cancelClickListener, Context context)
        {
            AlertDialog.Builder alertDialogBuilder = new AlertDialog.Builder(context);

            alertDialogBuilder.SetIcon(Resource.Drawable.ic_call_black_24dp);
            alertDialogBuilder.SetTitle("Connect to a room");
            alertDialogBuilder.SetPositiveButton("Connect", callParticipantsClickListener);
            alertDialogBuilder.SetNegativeButton("Cancel", cancelClickListener);
            alertDialogBuilder.SetCancelable(false);

           SetRoomNameFieldInDialog(participantEditText, alertDialogBuilder, context);

            return alertDialogBuilder.Create();
        }
        
        private static void SetRoomNameFieldInDialog(EditText roomNameEditText, AlertDialog.Builder alertDialogBuilder, Context context)
        {
            
            roomNameEditText.Hint = "room code";
            int horizontalPadding = context.Resources.GetDimensionPixelOffset(Resource.Dimension.activity_horizontal_margin);
            int verticalPadding = context.Resources.GetDimensionPixelOffset(Resource.Dimension.activity_vertical_margin);
            alertDialogBuilder.SetView(roomNameEditText, horizontalPadding, verticalPadding, horizontalPadding, 0);
        }
    }
}