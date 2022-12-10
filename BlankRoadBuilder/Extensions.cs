using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

using UnityEngine;

namespace BlankRoadBuilder;

public static class Extensions
{
	public enum DateFormat { DMY, MDY, TDMY }

	public static bool HasFlag(this Enum variable, Enum value)
	{
		if (variable == null)
			return false;

		if (value == null)
			throw new ArgumentNullException("value");

		var num = Convert.ToInt64(value);

		return (Convert.ToInt64(variable) & num) == num;
	}

	private static readonly BindingFlags staticFlags = BindingFlags.Instance | BindingFlags.NonPublic;

	public static void RaiseEvent(this object instance, string eventName, EventArgs e)
	{
		var type = instance.GetType();
		var eventField = type.GetField(eventName, staticFlags);
		if (eventField == null)
			throw new Exception($"Event with name {eventName} could not be found.");
		var multicastDelegate = eventField.GetValue(instance) as MulticastDelegate;
		if (multicastDelegate == null)
			return;

		var invocationList = multicastDelegate.GetInvocationList();

		foreach (var invocationMethod in invocationList)
			invocationMethod.DynamicInvoke(new[] { instance, e });
	}

	public static string ToRelatedString(this DateTime dt, bool shorter = false, bool longWords = true, bool utc = false)
	{
		var ts = new TimeSpan(Math.Abs(dt.Ticks - DateTime.Now.Ticks));
		var past = dt < (utc ? DateTime.UtcNow : DateTime.Now);
		var today = (utc ? DateTime.UtcNow : DateTime.Now).Date;

		if (ts.TotalHours < 5)
		{
			if (past)
				return ts.ToReadableString(shorter, longWords) + " ago";
			return "in " + ts.ToReadableString(shorter, longWords);
		}
		else if (dt.Date == today)
		{
			return $"Today at {dt:h:mm tt}";
		}
		else if (dt.Date == today.AddDays(1) || dt.Date == today.AddDays(-1))
		{
			return (past ? "Yesterday at " : "Tomorrow at ") + dt.ToString("h:mm tt");
		}
		else if (ts.TotalDays < 7)
		{
			return $"{(past ? "Last " : "Next ")}{dt.DayOfWeek} at {dt:h:mm tt}";
		}

		var days = ts.Days;
		var years = days / 365;
		days -= years * 365;
		var months = days / 30;

		if (years > 0)
			return (past ? $"{years} years ago" : $"in {years} years") + $" on {dt.ToReadableString(true, DateFormat.TDMY)}";

		if (months > 0)
			return (past ? $"{months} months ago" : $"in {months} months") + $" on {dt.ToReadableString(false, DateFormat.TDMY)}";

		return (past ? $"{days} days ago" : $"in {days} days") + $" on {dt.ToReadableString(false, DateFormat.TDMY)}";
	}

	public static string ToReadableString(this TimeSpan TS, bool shorter = false, bool longWords = true)
	{
		try
		{
			if (TS.Ticks != 0)
			{
				var sb = new List<string>();
				var days = TS.Days;
				var years = days / 365;
				days -= years * 365;
				var months = days / 30;
				days -= months * 30;
				var large = years > 0 || months > 1;

				if (large)
				{
					if (years != 0)
						sb.Add(years + " year".Plural(years));
					if (months != 0)
						sb.Add(months + " month".Plural(months));
					if (days != 0)
						sb.Add(days + " day".Plural(days));
				}
				else
				{
					if (TS.Days != 0)
						sb.Add(TS.Days + (!longWords ? "d" : " day".Plural(TS.Days)));
					if (TS.Hours != 0)
						sb.Add(TS.Hours + (!longWords ? "h" : " hour".Plural(TS.Hours)));
					if (TS.Minutes != 0)
						sb.Add(TS.Minutes + (!longWords ? "m" : " minute".Plural(TS.Minutes)));
					if ((TS.Seconds != 0 || TS.TotalSeconds < 1) && (!shorter || TS.TotalSeconds < 60))
						sb.Add((TS.TotalSeconds < 1 ? Math.Round(TS.TotalSeconds, 2) : TS.Seconds) + (!longWords ? "s" : " second".Plural(TS.Seconds)));
				}

				if (shorter)
					return string.Join(", ", sb.Take(2).ToArray());

				return string.Join(", ", sb.ToArray());
			}
		}
		catch
		{ }

		return !longWords ? "0s" : "0 seconds";
	}
	public static string ToReadableString(this DateTime DT, bool AddYear = true, DateFormat format = DateFormat.DMY, bool fullMonth = true)
	{
		var day = "th";
		var month = DT.ToString(fullMonth ? "MMMM" : "MMM");

		if (DT.Day < 4 || DT.Day > 20)
			switch (DT.Day % 10)
			{
				case 1:
					day = "st";
					break;
				case 2:
					day = "nd";
					break;
				case 3:
					day = "rd";
					break;
			}

		switch (format)
		{
			case DateFormat.DMY:
			default:
				return $"{DT.Day}{day} of {month}{(AddYear ? $" {DT.Year}" : "")}";

			case DateFormat.MDY:
				return $"{month} {DT.Day}{day}{(AddYear ? $" of {DT.Year}" : "")}";

			case DateFormat.TDMY:
				return $"the {DT.Day}{day} of {month}{(AddYear ? $" {DT.Year}" : "")}";
		}
	}

	public static string Plural(this string @base, int plural, string addition = "s")
		=> plural != 1 ? $"{@base}{addition}" : @base;

	public static bool SpellCheck(this string s1, string s2, int maxErrors = 2, bool caseCheck = true)
	{
		return s1.SpellCheck(s2, caseCheck) <= maxErrors;
	}

	public static bool SearchCheck(this string s1, string s2, bool caseCheck = false)
	{
		return s1.SpellCheck(s2, caseCheck) <= (int)Math.Ceiling((decimal)(s2.Length - 4) / 5m) || (caseCheck ? s1 : s1.ToLower()).Contains(caseCheck ? s2 : s2.ToLower()) || (s1.AbbreviationCheck(s2) && s2.GetWords().Length > 2);
	}

	public static int SpellCheck(this string s1, string s2, bool caseCheck = true)
	{
		s1 = s1.RemoveDoubleSpaces();
		s2 = s2.RemoveDoubleSpaces();
		if (!caseCheck)
		{
			s1 = s1.ToLower();
			s2 = s2.ToLower();
		}

		int length = s1.Length;
		int length2 = s2.Length;
		int[,] array = new int[length + 1, length2 + 1];
		if (length == 0)
		{
			return length2;
		}

		if (length2 == 0)
		{
			return length;
		}

		int num = 0;
		while (num <= length)
		{
			array[num, 0] = num++;
		}

		int num2 = 0;
		while (num2 <= length2)
		{
			array[0, num2] = num2++;
		}

		for (int i = 1; i <= length; i++)
		{
			for (int j = 1; j <= length2; j++)
			{
				int num3 = ((s2[j - 1] != s1[i - 1]) ? 1 : 0);
				array[i, j] = Math.Min(Math.Min(array[i - 1, j] + 1, array[i, j - 1] + 1), array[i - 1, j - 1] + num3);
			}
		}

		return array[length, length2];
	}

	public static string RemoveDoubleSpaces(this string S)
	{
		return Regex.Replace(S, " {2,}", " ");
	}

	public static bool AbbreviationCheck(this string string1, string string2)
	{
		if (string.IsNullOrEmpty(string1) || string.IsNullOrEmpty(string1))
		{
			return false;
		}

		string1 = string1.ToLower().Replace("'s ", " ");
		string2 = string2.ToLower().Replace("'s ", " ");
		return string1.Where((char x) => x != ' ') == string2.GetAbbreviation() || string1.GetAbbreviation() == string2.Where((char x) => x != ' ');
	}

	public static string GetAbbreviation(this string S)
	{
		StringBuilder stringBuilder = new StringBuilder();
		string[] words = S.GetWords(includeNumbers: true);
		foreach (string text in words)
		{
			stringBuilder.Append(text.All((char x) => char.IsDigit(x)) ? text : text[0].ToString());
		}

		if (Regex.IsMatch(stringBuilder.ToString(), "^[A-z]+[0-9]+$"))
		{
			Match match = Regex.Match(stringBuilder.ToString(), "^([A-z]+)([0-9]+)$");
			return $"{match.Groups[1]} {match.Groups[2]}";
		}

		return stringBuilder.ToString();
	}

	public static string[] GetWords(this string S, bool includeNumbers = false)
	{
		return (S == string.Empty) ? new string[0] : Regex.Matches(S, "\\b" + (includeNumbers ? "" : "(?![0-9])") + "(\\w+)(?:'\\w+)?\\b").Cast<Match>().Select(x => x.Groups[1].Value).ToArray();
	}

	public static string FormatWords(this string str, bool forceUpper = false)
	{
		str = str.RegexReplace("([a-z])([A-Z])", (Match x) => x.Groups[1].Value + " " + x.Groups[2].Value, ignoreCase: false).RegexReplace("(\\b)(?<!')([a-z])", (Match x) => x.Groups[1].Value + x.Groups[2].Value.ToUpper(), ignoreCase: false);
		if (forceUpper)
		{
			str = str.RegexReplace("(^[a-z])|(\\ [a-z])", (Match x) => x.Value.ToUpper());
		}

		return str;
	}

	public static string RegexReplace(this string @base, string pattern, MatchEvaluator replacement = null, bool ignoreCase = true)
	{
		return ignoreCase ? Regex.Replace(@base, pattern, replacement, RegexOptions.IgnoreCase) : Regex.Replace(@base, pattern, replacement);
	}

	public static string RegexReplace(this string @base, string pattern, string replacement = "", bool ignoreCase = true)
	{
		return (@base == null) ? null : (ignoreCase ? Regex.Replace(@base, pattern, replacement, RegexOptions.IgnoreCase) : Regex.Replace(@base, pattern, replacement, RegexOptions.IgnoreCase));
	}

	public static Texture2D Color(this Texture2D texture, Color32 color, bool createNewTexture = true)
	{
		var pixels = texture.GetPixels32();

		for (var i = 0; i < pixels.Length; i++)
		{
			pixels[i] = new Color32(color.r, color.g, color.b, pixels[i].a);
		}

		var newTexture = createNewTexture ? new Texture2D(texture.width, texture.height, TextureFormat.ARGB32, mipmap: false, linear: false) : texture;

		newTexture.SetPixels32(pixels);
		newTexture.Apply(updateMipmaps: false);

		return newTexture;
	}
}