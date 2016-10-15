using System;
using System.Collections.Generic;

namespace MySqlTool.Class
{
	public class Speed
	{
		private Queue<SpeedInfo> m_Queue = new Queue<SpeedInfo>();

		private int m_QueueCount = 3;

		public int QueueCount
		{
			get
			{
				return this.m_QueueCount;
			}
			set
			{
				this.m_QueueCount = value;
			}
		}

		public long CurrSpeed
		{
			get
			{
				SpeedInfo[] array = this.m_Queue.ToArray();
				int num = array.Length;
				long result;
				if (num == 0)
				{
					result = 0L;
				}
				else
				{
					SpeedInfo speedInfo = array[0];
					SpeedInfo speedInfo2 = array[num - 1];
					double totalSeconds = (speedInfo2.Date - speedInfo.Date).TotalSeconds;
					if (totalSeconds == 0.0)
					{
						result = 0L;
					}
					else
					{
						result = Convert.ToInt64((double)(speedInfo2.Num - speedInfo.Num) / totalSeconds);
					}
				}
				return result;
			}
		}

		public void Add(long value)
		{
			this.Add(new SpeedInfo
			{
				Date = DateTime.Now,
				Num = value
			});
		}

		public void Add(SpeedInfo info)
		{
			this.m_Queue.Enqueue(info);
			if (this.m_Queue.Count > 3)
			{
				this.m_Queue.Dequeue();
			}
		}
	}
}
