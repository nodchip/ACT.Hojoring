using ACT.SpecialSpellTimer.RaidTimeline;
using FFXIV.Framework.Globalization;
using FFXIV.Framework.XIVHelper;
using System.Text.RegularExpressions;

/// <summary>
/// 「次元の狭間オメガ零式：シグマ編1」のScritptedタイムライン
/// </summary>
public class raids_Sigmascape_V1_0_Savage : TimelineXBase
{
	/// <summary>
	/// 後半待機フェーズに切り替えるためのログにマッチする正規表現
	/// </summary>
	private static readonly Regex RegexForWaitForSecondHalf = new Regex("魔列車は「酸性雨」の構え。", RegexOptions.Compiled);

	/// <summary>
	/// 後半フェーズに切り替えるためのログにマッチする正規表現
	/// </summary>
	private static readonly Regex RegexForSecondHalf = new Regex("魔列車は「魔界の前照灯」の構え。", RegexOptions.Compiled);

	private int _numAcidRain = 0;
	private int _devildomHeadlight = 0;

	public override TimelineModel InitialTimeline
	{
		get
		{
			// 毎回newしないとインスタンスが使い回されるせいか意図通り動かない。
			return new TimelineModel()
			{
				Name = "次元の狭間オメガ零式：シグマ編1",
				Revision = "rev3",
				Description = "シグマ1層向けのタイムラインです。\n既存のスペスペたいむをScriptedタイムラインに変換するとこのような感じになると思います。",
				Author = "nodchip and anoyetta with Hojoring Forum",
				Zone = "Sigmascape V1.0 (Savage)",
				Locale = Locales.JA,
				StartTrigger = "0039::戦闘開始！",
				Activities = new[]
				{
					A(time:"12", text:"固定ゴーストポップ"),
					A(time:"28", text:"セイントビーム", sync:"魔列車の「セイントビーム」", icon:"Marker.png"),
					A(time:"30", text:"魔界の汽笛(飛)", sync:"魔列車は「魔界の汽笛」の構え。", notice:"ノックバックゴースト", icon:"KnockBack.png"),
					A(time:"37", text:"念力", sync:"未練のゴーストは「念力」の構え。", icon:"KnockBack.png"),
					A(time:"47", text:"魔霊撃", sync:"魔列車は「魔霊撃」の構え。", notice:"次は、まれーげき", icon:"HardAttack.png"),
					A(time:"56", text:"追突", sync:"魔列車は「追突」の構え。", notice:"次は、追突。", icon:"KnockBack.png"),
					A(time:"67", text:"酸性雨", sync:"魔列車は「酸性雨」の構え。", notice:"次は、酸性雨。", icon:"AllRangeAttack.png"),
					A(time:"78", text:"魔界の汽笛(横)", sync:"魔列車は「魔界の汽笛」の構え。", notice:"横からゴースト", icon:"Attention.png"),
					A(time:"99", text:"魔界の前照灯", sync:"魔列車は「魔界の前照灯」の構え。", notice:"次は、ビーム。", icon:"DamageShare.png"),
					A(time:"114", text:"魔界の汽笛(追)", sync:"魔列車は「魔界の汽笛」の構え。", notice:"追跡ゴースト", icon:"StackAOE.png"),
					A(time:"134", text:"セイントビーム", sync:"魔列車の「セイントビーム」", icon:"Marker.png"),
					A(time:"140", text:"魔界の光(MT+1)", sync:"に「マーキング」の効果。", notice:"スイッチ", icon:"Marker.png"),
					A(time:"147", text:"魔霊撃", sync:"魔列車は「魔霊撃」の構え。", notice:"次は、まれーげき。", icon:"HardAttack.png"),
					A(time:"154", text:"酸性雨", sync:"魔列車は「酸性雨」の構え。", notice:"次は、酸性雨。", icon:"AllRangeAttack.png"),
				}
			};
		}
	}

	public override void ProcessLogLine(XIVLog log)
	{
		// 後半待機フェーズに入るかどうか判定する。
		if (RegexForWaitForSecondHalf.IsMatch(log.LogLine))
		{
			++_numAcidRain;

			// 2 回目の酸性雨かどうか
			if (_numAcidRain == 2)
			{
				// 後半待機フェーズのタイムラインを設定する。
				SetTimeline(
					A(time: "100", text: "後半まで待機1"),
					A(time: "200", text: "後半まで待機2"),
					A(time: "300", text: "後半まで待機3"),
					A(time: "400", text: "後半まで待機4")
				);
			}
		}

		// 後半フェーズに入るかどうか判定する。
		if (RegexForSecondHalf.IsMatch(log.LogLine))
		{
			++_devildomHeadlight;

			if (_devildomHeadlight == 2)
			{
				// 後半フェーズのタイムラインを設定する。
				SetTimeline(
					A(time: "000", text: "魔界の光(ヒラ)", sync: "に「マーキング」の効果。", icon: "Marker.png"),
					A(time: "018", text: "魔霊撃", sync: "魔列車は「魔霊撃」の構え。", notice: "次は、まれーげき。", icon: "HardAttack.png"),
					A(time: "030", text: "魔界の汽笛(追)", sync: "魔列車は「魔界の汽笛」の構え。", notice: "追跡ゴースト", icon: "StackAOE.png"),
					A(time: "047", text: "セイントビーム", sync: "魔列車の「セイントビーム」", icon: "Marker.png"),
					A(time: "053", text: "魔界の汽笛(飛)", sync: "魔列車は「魔界の汽笛」の構え。", notice: "ノックバックゴースト", icon: "KnockBack.png"),
					A(time: "060", text: "念力", sync: "未練のゴーストは「念力」の構え。", icon: "KnockBack.png"),
					A(time: "062", text: "魔界の前照灯", sync: "魔列車は「魔界の前照灯」の構え。", notice: "次は、ビーム。", icon: "DamageShare.png"),
					A(time: "077", text: "追突", sync: "魔列車は「追突」の構え。", notice: "次は、追突。", icon: "KnockBack.png"),

					A(time: "085", text: "固定ゴーストポップ"),
					A(time: "096", text: "セイントビーム", sync: "魔列車の「セイントビーム」", icon: "Marker.png"),
					A(time: "102", text: "魔界の汽笛(横)", sync: "魔列車は「魔界の汽笛」の構え。", notice: "横からゴースト", icon: "Attention.png"),
					A(time: "106", text: "魔界の前照灯", sync: "魔列車は「魔界の前照灯」の構え。", notice: "次は、ビーム。", icon: "DamageShare.png"),
					A(time: "113", text: "魔界の光(x4)", icon: "Marker.png"),
					A(time: "119", text: "魔界の汽笛(追)", sync: "魔列車は「魔界の汽笛」の構え。", notice: "追跡ゴースト", icon: "StackAOE.png"),
					A(time: "136", text: "セイントビーム", sync: "魔列車の「セイントビーム」", icon: "Marker.png"),
					A(time: "138", text: "魔界の光(MT+1)", sync: "に「マーキング」の効果。", notice: "スイッチ", icon: "Marker.png"),
					A(time: "145", text: "魔霊撃", sync: "魔列車は「魔霊撃」の構え。", notice: "次は、まれーげき。", icon: "HardAttack.png"),
					A(time: "152", text: "酸性雨", sync: "魔列車は「酸性雨」の構え。", notice: "次は、酸性雨。", icon: "AllRangeAttack.png"),
					A(time: "165", text: "魔界の前照灯", sync: "魔列車は「魔界の前照灯」の構え。", notice: "次は、ビーム。", icon: "DamageShare.png"),
					A(time: "173", text: "酸性雨", sync: "魔列車は「酸性雨」の構え。", notice: "次は、酸性雨。", icon: "AllRangeAttack.png"),
					A(time: "190", text: "魔界の汽笛(追)", sync: "魔列車は「魔界の汽笛」の構え。", notice: "追跡ゴースト", icon: "StackAOE.png"),

					A(time: "192", text: "固定ゴーストポップ"),
					A(time: "211", text: "魔界の光(MT+1)", sync: "に「マーキング」の効果。", notice: "スイッチ", icon: "Marker.png"),
					A(time: "211", text: "酸性雨", sync: "魔列車は「酸性雨」の構え。", notice: "次は、酸性雨。", icon: "AllRangeAttack.png"),
					A(time: "218", text: "魔霊撃", sync: "魔列車は「魔霊撃」の構え。", notice: "次は、まれーげき。", icon: "HardAttack.png"),
					A(time: "233", text: "魔界の汽笛(飛)", sync: "魔列車は「魔界の汽笛」の構え。", notice: "ノックバックゴースト", icon: "KnockBack.png"),
					A(time: "242", text: "魔界の前照灯", sync: "魔列車は「魔界の前照灯」の構え。", notice: "次は、ビーム。", icon: "DamageShare.png"),
					A(time: "255", text: "追突", sync: "魔列車は「追突」の構え。", notice: "次は、追突。", icon: "KnockBack.png"),
					A(time: "266", text: "酸性雨", sync: "魔列車は「酸性雨」の構え。", notice: "次は、酸性雨。", icon: "AllRangeAttack.png"),

					A(time: "277", text: "固定ゴーストポップ"),
					A(time: "283", text: "魔界の汽笛(横)", sync: "魔列車は「魔界の汽笛」の構え。", notice: "横からゴースト", icon: "Attention.png"),
					A(time: "287", text: "魔界の前照灯", sync: "魔列車は「魔界の前照灯」の構え。", notice: "次は、ビーム。", icon: "DamageShare.png"),
					A(time: "300", text: "魔界の光(x4)", icon: "Marker.png"),
					A(time: "304", text: "時間切れ", sync: "魔列車の「セイントビーム」", icon: "Timeout.png")
				);
			}
		}
	}
}
