#nullable enable
using IntelOrca.PvZTools.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO.Pipes;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace IntelOrca.PvZTools
{
	public partial class MainForm : Form
	{
		PvZProcess mProcess = null!;
		ZombieSpawner? mSpawner;
		ZombieProbabilityForm mZPF = new ZombieProbabilityForm();

		Random mRand = new Random();

        int totalLikes = 0;
        int currentLikes = 0;

		public MainForm()
		{
			try
			{
				mProcess = new PvZProcess();
			}
			catch (NullReferenceException e)
			{
				MessageBox.Show(e.Message + " If you have one, please report this as a bug at https://github.com/IntelOrca/PVZTools/issues.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				Environment.Exit(1);
			}


            InitializeComponent();

			this.Icon = Resources.orca_icon;

			chkPool.Checked = true;

			for (int i = 0; i < Zombie.szZombieTypes.Length; i++)
				cmbZombieType.Items.Add(Zombie.szZombieTypes[i]);

			cmbZombieType.SelectedIndex = 0;

			tmrSpawn.Interval = 2000;

			//CreateSliderGroup();

			ReceiveGiftData();
			Console.WriteLine("Interact or Gift to Spawn Zombies");
            Console.WriteLine("Every 250 Total likes a zombie will spawn");

            UpdateStatus();
		}

		private void ReceiveGiftData()
		{
			Task.Run(() =>
			{
				using (NamedPipeServerStream pipeServer = new NamedPipeServerStream("PIPE", PipeDirection.In))
				{
					while (true)
					{
						pipeServer.WaitForConnection();

						byte[] dataBytes = new byte[1024];
						int bytesRead = pipeServer.Read(dataBytes, 0, dataBytes.Length);
						string data = Encoding.UTF8.GetString(dataBytes, 0, bytesRead);

						ProcessPipeData(data);

						pipeServer.Disconnect();
					}
				}
			});
		}

        //Takes pipe data, sends console message, and spawns the appropriate zombie
        //data should be [type cost sender giftName] <-for gift, rest are [code sender|likeAmount]
        private void ProcessPipeData(string data)
		{
            //Console.WriteLine(data);
            string[] splitData = data.Split(' ');
			string type = splitData[0];
            string cost = splitData[1];
			string sender = "";
            string name = "";
            if (splitData.Length > 2)
			{
				sender = splitData[2];
				name = splitData[3];
				for (int i = 4; i < splitData.Length; i++)
				{
					name += " " + splitData[i];
				} 
			}
            int typeNumber = int.Parse(type);
            switch (typeNumber)
            {

                //On Join
                case -3:
                    SpawnZombieByNumber(4);
                    SetConsoleColor(ConsoleColor.Yellow);
                    Console.WriteLine($"{cost} joined!");
                    Console.WriteLine($"Spawning:Buckethead");
                    Console.WriteLine($"==================================");
                    SetConsoleColor(ConsoleColor.White);
                    break;
                //On Share
                case -2:
                    SpawnZombieByNumber(29);
                    SetConsoleColor(ConsoleColor.Yellow);
                    Console.WriteLine($"{cost} shared! Thank You!!");
                    Console.WriteLine($"Spawning:Peashooter Zombie");
                    Console.WriteLine($"==================================");
                    SetConsoleColor(ConsoleColor.White);
                    break;
                //On Follow
                case -1:
                    SpawnZombieByNumber(29);
                    SetConsoleColor(ConsoleColor.Yellow);
                    Console.WriteLine($"{cost} followed! Thank You!!");
                    Console.WriteLine($"Spawning:Gatling Zombie");
                    Console.WriteLine($"==================================");
                    SetConsoleColor(ConsoleColor.White);
                    break;
				//On Like
                case 0:
                    totalLikes += int.Parse(splitData[1]);
                    currentLikes += int.Parse(splitData[1]);
                    if (currentLikes > 250)
                    {
                        SpawnZombieByNumber(26);
                        SetConsoleColor(ConsoleColor.White);
                        Console.WriteLine($"Likes:{totalLikes} Spawning:Peashooter");
                        Console.WriteLine($"==================================");
                        SetConsoleColor(ConsoleColor.White);
                        currentLikes = 0;
                    }
                    break;
				//On Gift
				default:
					string zombieName = SpawnZombieByCost(int.Parse(splitData[1]));
                    SetConsoleColor(ConsoleColor.Cyan);
                    Console.WriteLine($"THANK YOU: {sender}");
                    Console.WriteLine($"Gift:{name} Spawning:{zombieName}");
                    Console.WriteLine($"==================================");
                    SetConsoleColor(ConsoleColor.White);
                    break;
            }
        }

        private static void SetConsoleColor(ConsoleColor color)
        {
            if (Console.ForegroundColor != color)
                Console.ForegroundColor = color;
        }

		private List<(Func<int, bool> Condition, (int, string) Value)> conditions =
			new List<(Func<int, bool> Condition, (int, string) Value)>
			{
				(x => x == 1, (7, "Football")),
				(x => x == 5, (8, "Dancer")),
                (x => x == 10, (22, "Catapult")),
                (x => x == 20, (12, "Zomboni")),
                (x => x == 30, (19, "Yeti")),
                (x => x == 99, (23, "Gargantuar")),
                (x => x == 100, (32, "Giga-Gargantuar")),
                (x => x >= 500, (25, "Dr. Zomboss")),
				(x => x < 1, (0, "Normal")),
            };

        private string SpawnZombieByCost(int cost)
        {
            var value = conditions.First(tuple => tuple.Condition(cost));
            mSpawner!.Spawn(value.Item2.Item1, GetRandomRowFromSelection(value.Item2.Item1));
			return value.Item2.Item2;
        }

        private void SpawnZombieByNumber(int number)
		{
			mSpawner!.Spawn(number, GetRandomRowFromSelection(number));
		}

        private void SpawnZombie()
		{
			int zombieType;
			if (rdoProbability.Checked)
				zombieType = mZPF.GetRandomZombieType();
			else
				zombieType = GetZombieType();
			
			mSpawner!.Spawn(zombieType, GetRandomRowFromSelection(zombieType));
		}

		private void btnSpawnZombie_Click(object sender, EventArgs e)
		{
			SpawnZombie();
		}

		private void chkActive_CheckedChanged(object sender, EventArgs e)
		{
			tmrSpawn.Enabled = chkActive.Checked;
		}

		private int GetZombieType()
		{
			return cmbZombieType.SelectedIndex;
		}

		private int GetRandomRowFromSelection(int type = -1)
		{
			List<int> lanes = new List<int>();
			for (int i = 0; i < 6; i++) {
				if ((i == 2 || i == 3) && type != -1 && chkPool.Checked)
					if (!Zombie.CanZombieSwim[type])
						continue;

				string ctlName = "chkLane" + i;
				CheckBox ctl = (CheckBox)grpLanes.Controls[ctlName];
				if (ctl.Checked)
					lanes.Add(i);
			}

			if (lanes.Count == 0)
				return -1;

			return lanes[mRand.Next(0, lanes.Count)];
		}

		private void tmrSpawn_Tick(object sender, EventArgs e)
		{
			SpawnZombie();
		}

		private void txtInterval_TextChanged(object sender, EventArgs e)
		{
			chkActive.Checked = false;

			foreach (Char c in txtInterval.Text) {
				if (!Char.IsNumber(c)) {
					txtInterval.ForeColor = Color.Red;
					return;
				}
			}

			int result;
			if (Int32.TryParse(txtInterval.Text, out result)) {
				txtInterval.ForeColor = Color.Black;
				try {
					tmrSpawn.Interval = result;
				} catch {
					txtInterval.ForeColor = Color.Red;
				}
			} else {
				txtInterval.ForeColor = Color.Red;
			}
		}

		private void btnEditProbabilities_Click(object sender, EventArgs e)
		{
			mZPF.Show();
		}

		private void chkPool_CheckedChanged(object sender, EventArgs e)
		{
			chkLane2.ForeColor = chkLane3.ForeColor = (chkPool.Checked ? Color.Blue : Color.Black);
		}

		private void tmrStatusUpdate_Tick(object sender, EventArgs e)
		{
			UpdateStatus();
		}

		private void UpdateStatus()
		{
			if (mProcess.HasProcess)
				return;

			if (!mProcess.OpenProcess()) {
				lblStatus.Text = "Status: Unable to connect...";
				btnSpawnZombie.Enabled = chkActive.Enabled = chkActive.Checked = false;
			} else {
				lblStatus.Text = "Status: Running...";
				btnSpawnZombie.Enabled = chkActive.Enabled = true;
				mSpawner = new ZombieSpawner(mProcess);
				mSpawner.Activate();
			}
		}

		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			if (mSpawner != null)
				if (mSpawner.Active)
					mSpawner.Deactivate();

			base.OnClosing(e);
		}

		private void lblAuthor_MouseEnter(object sender, EventArgs e)
		{
			lblAuthor.Font = new Font(lblAuthor.Font, FontStyle.Underline);
		}

		private void lblAuthor_MouseLeave(object sender, EventArgs e)
		{
			lblAuthor.Font = new Font(lblAuthor.Font, FontStyle.Regular);
		}

		private void lblAuthor_MouseDown(object sender, MouseEventArgs e)
		{
			Process.Start("http://intelorca.co.uk");
		}
	}
}
