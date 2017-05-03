using Finanse.DataAccessLayer;

namespace Finanse.Models.MAccounts {
    public class SubMAccount : MAccount {

        public string BossAccountGlobalId { get; set; }

        public MAccount ParentAccount => MAccountsDal.GetAccountByGlobalId(BossAccountGlobalId);


        public override int GetHashCode() => Name.GetHashCode() * Id;

        public override bool Equals(object o) {
            if (!( o is SubMAccount ))
                return false;

            return
                base.Equals((MAccount)o) &&
                ( (SubMAccount)o ).BossAccountGlobalId == BossAccountGlobalId;
        }
    }
}
