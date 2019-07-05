using Network;
using System.Collections;

namespace Data
{
    public class Account
    {
        public long accountId;

        public string userID;

        public string username;

        public string password;

        public int sessionID { get { return ClientTcpMessage.sessionId; } set { ClientTcpMessage.sessionId = value; } }

        //MAC, session id

        public void OnChangeAccountId( System.Object obj )
        {
            this.accountId = (long)obj;
        }
    }
}
