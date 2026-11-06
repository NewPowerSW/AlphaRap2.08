using System.Threading.Tasks;

namespace NPSDK
{
	public class IOBaseInterface
	{
		public virtual bool Initial(string Port, string ip, string fieldName)
		{
			return false;
		}

		public virtual bool GetState()
		{
			return false;
		}

		public virtual bool On()
		{
			return false;
		}

		public virtual async Task<bool> On_Async()
		{
			return false;
		}

		public virtual bool Off()
		{
			return false;
		}

		public virtual async Task<bool> Off_Async()
		{
			return false;
		}

		public virtual bool IsOn()
		{
			return false;
		}

		public virtual bool IsOff()
		{
			return false;
		}

		public virtual void Dispose()
		{
		}
	}
}
