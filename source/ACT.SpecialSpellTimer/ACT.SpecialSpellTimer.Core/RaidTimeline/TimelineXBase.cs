using ACT.SpecialSpellTimer.Utility;
using FFXIV.Framework.XIVHelper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace ACT.SpecialSpellTimer.RaidTimeline
{
	/// <summary>
	/// Scriptedタイムラインのインターフェース。
	/// </summary>
	public abstract class TimelineXBase
	{
		public TimelineController Controller { get; set; }

		/// <summary>
		/// タイムラインの初期状態を返す。スペスペたいむのヘッダー情報や、戦闘開始直後のタイムライン等を含むタイムラインを返す。
		/// </summary>
		public abstract TimelineModel InitialTimeline { get; }

		/// <summary>
		/// XIVLogが1行流れてくる毎の処理を記述する。フェーズ変更処理等はこちらで行う。
		/// </summary>
		/// <param name="log">ログ</param>
		public abstract void ProcessLogLine(XIVLog log);

		/// <summary>
		/// Activityを生成する。
		/// TODO(someone): マッチ時に呼び出されるActionを指定できるようにする。
		/// TODO(someone): パラメーターのデフォルト値をTimelineModel.DefaultValues.csのルーチンを利用して設定する方法を考える。
		/// </summary>
		/// <param name="name"></param>
		/// <param name="inherits"></param>
		/// <param name="enabled"></param>
		/// <param name="time"></param>
		/// <param name="text"></param>
		/// <param name="sync"></param>
		/// <param name="sync_s"></param>
		/// <param name="sync_e"></param>
		/// <param name="notice"></param>
		/// <param name="notice_d"></param>
		/// <param name="notice_o"></param>
		/// <param name="notice_vol"></param>
		/// <param name="notice_sync"></param>
		/// <param name="style"></param>
		/// <param name="icon"></param>
		/// <param name="exec"></param>
		/// <param name="args"></param>
		/// <param name="json"></param>
		/// <returns></returns>
		protected static TimelineActivityModel A(
			string name = null,
			string inherits = null,
			string enabled = "true",
			string time = null,
			string text = null,
			string sync = null,
			string sync_s = "-12",
			string sync_e = "12",
			string notice = null,
			string notice_d = "Both",
			string notice_o = "-6",
			string notice_vol = "1.0",
			string notice_sync = "false",
			string style = null,
			string icon = null,
			string exec = null,
			string args = null,
			string json = null)
		{
			return new TimelineActivityModel
			{
				Name = name,
				Inherits = inherits,
				EnabledXML = enabled,
				TimeText = time,
				Text = text,
				SyncKeyword = sync,
				SyncOffsetStartXML = sync_s,
				SyncOffsetEndXML = sync_e,
				Notice = notice,
				NoticeDeviceXML = notice_d,
				NoticeOffsetXML = notice_o,
				NoticeVolumeXML = notice_vol,
				NoticeSyncXML = notice_sync,
				Style = style,
				Icon = icon,
				ExecuteFileName = exec,
				Arguments = args,
				Json = json,
				// HACK: 以下の行がないと、巨大なアイコンが表示されてしまう。UIフレームワークのデフォルト値が使われてしまっているためだと思う。
				// TODO(someone): スタイルを正しく設定する方法を調べて、修正する。
				//StyleModel = new TimelineStyle(),
			};
		}

		/// <summary>
		/// アクティビティとトリガーをセットする。スペスペたいむのgotoに相当する。
		/// </summary>
		/// <param name="activitiesAndTriggers">アクティビティとトリガー</param>
		protected void SetTimeline(params TimelineBase[] activitiesAndTriggers)
		{
			Debug.Assert(Controller != null);

			// 競合状態を防ぐため、ControllerのActivityLineにアクセスする前に、lockを取らなければならない。
			// また、ControllerのRemoveAllActivity()は、メインスレッドからしかアクセスできない。
			// さらに、ControllerのRemoveAllActivity()の中でlockがとられている。
			// 呼び出し元のスレッドはTask.Run()で作られたため、
			// ここでlockしてからApplication.Current.Dispatcher.Invoke()内でRemoveAllActivity()を呼び出すとデッドロックする。
			// 仕方ないので、以下のロジック全体をApplication.Current.Dispatcher.Invoke()内で行う。
			Application.Current.Dispatcher.Invoke(() =>
			{
				lock (Controller)
				{
					var currentActivity = Controller.ActivityLine
						.Where(activity => activity.IsActive && !activity.IsDone && activity.Time <= Controller.CurrentTime)
						.OrderByDescending(activity => activity.Seq)
						.FirstOrDefault();

					var currentIndex = Controller.ActivityLine.IndexOf(currentActivity);
					var currnetSeq = 1;

					if (currentActivity != null)
					{
						currentIndex = Controller.ActivityLine.IndexOf(currentActivity);
						currnetSeq = currentActivity.Seq;
					}

					// タイムライン中のトリガのカウンタを初期化する
					var triggers = activitiesAndTriggers
						.Where(model => model.TimelineType == TimelineElementTypes.Trigger)
						.Cast<TimelineTriggerModel>()
						.OrderBy(model => model.No.GetValueOrDefault())
						.Where(model => model.Enabled.GetValueOrDefault());
					foreach (var trigger in triggers)
					{
						trigger.Init();
					}
					// TODO(nodchip): このままだとtriggerがTimelineModelに含まれず、動かない気がする。triggerをTimelineModelに追加しないといけないか調べ、必要に応じて追加する。

					// タイムライン配下のActivityを取得する
					var activities = activitiesAndTriggers
						.Where(x => x.TimelineType == TimelineElementTypes.Activity)
						.Cast<TimelineActivityModel>()
						.OrderBy(x => x.Time)
						.Where(x => x.Enabled.GetValueOrDefault())
						.Select(x => x.Clone());

					if (!activities.Any())
					{
						return;
					}

					try
					{
						Controller.Model.StopLive();

						// 差し込まれる次のシーケンスを取得する
						var nextSeq = currnetSeq + 1;

						// 後のActivityを削除する
						Controller.RemoveAllActivity(x => x.Seq > currnetSeq);

						// 差し込むActivityにシーケンスをふる
						var toInsert = new List<TimelineActivityModel>();
						foreach (var activity in activities)
						{
							activity.Init(nextSeq++);
							activity.Time += Controller.CurrentTime;
							toInsert.Add(activity);
						}

						Controller.AddRangeActivity(toInsert);
					}
					finally
					{
						Controller.Model.ResumeLive();
					}
				}
			});
		}
	}
}