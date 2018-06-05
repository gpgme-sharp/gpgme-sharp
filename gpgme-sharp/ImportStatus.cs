using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Libgpgme.Interop;

namespace Libgpgme
{
    public class ImportStatus : IEnumerable<ImportStatus>
    {
        private string _fpr; // char *
        private ImportStatus _next;
        private int _result; //gpgme_error_t
        private uint _status;

        internal ImportStatus(IntPtr statusPtr) {
            if (statusPtr == IntPtr.Zero) {
                throw new InvalidPtrException("An invalid import status pointer has been given.");
            }

            UpdateFromMem(statusPtr);
        }

        public ImportStatus Next {
            get { return _next; }
        }

        /* Fingerprint.  */
        public String Fpr {
            get { return _fpr; }
        }

        /* If a problem occured, the reason why the key could not be
           imported.  Otherwise GPGME_No_Error.  */
        public int Result {
            get { return _result; }
        }

        /* The result of the import, the GPGME_IMPORT_* values bit-wise
           ORed.  0 means the key was already known and no new components
           have been added.  */

        /* The key was new.  */
        public bool NewKey {
            get { return ((_status & (uint) ImportStatusFlags.New) > 0); }
        }

        /* The key contained new user IDs.  */
        public bool HasNewUids {
            get { return ((_status & (uint) ImportStatusFlags.Uid) > 0); }
        }

        /* The key contained new signatures.  */
        public bool HasNewSignatures {
            get { return ((_status & (uint) ImportStatusFlags.Signature) > 0); }
        }

        /* The key contained new sub keys.  */
        public bool HasNewSubkeys {
            get { return ((_status & (uint) ImportStatusFlags.Subkey) > 0); }
        }

        /* The key contained a secret key.  */
        public bool NewSecretKey {
            get { return ((_status & (uint) ImportStatusFlags.Secret) > 0); }
        }
        #region IEnumerable<ImportStatus> Members

        public IEnumerator<ImportStatus> GetEnumerator() {
            ImportStatus st = this;
            while (st != null) {
                yield return st;
                st = st.Next;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        #endregion
        private void UpdateFromMem(IntPtr statusPtr) {
            var result = new _gpgme_import_status();
            Marshal.PtrToStructure(statusPtr, result);

            if (result.fpr != IntPtr.Zero) {
                _fpr = Gpgme.PtrToStringAnsi(result.fpr);
            }
            _status = result.status;
            _result = result.result;
            if (result.next != IntPtr.Zero) {
                _next = new ImportStatus(result.next);
            }
        }
    }
}