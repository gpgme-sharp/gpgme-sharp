namespace Libgpgme
{
    internal class PgpEnableDisableOptions
    {
        internal Mode OperationMode;
        internal bool cmdSend;
        #region Nested type: Mode

        internal enum Mode
        {
            Enable,
            Disable
        }

        #endregion
    }
}