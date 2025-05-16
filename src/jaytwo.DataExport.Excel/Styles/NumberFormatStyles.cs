namespace jaytwo.DataExport.Excel.Styles;

public enum NumberFormatStyles
{
    General = 0,
    NumberNoDecimals = 1,
    NumberDecimalTwoPlaces = 2,
    NumberThousandsSeparator = 3,
    NumberThousandsSeparatorTwoDecimals = 4,
    CurrencyNoDecimals = 5,
    CurrencyNoDecimalsRedNegative = 6,
    CurrencyTwoDecimals = 7,
    CurrencyTwoDecimalsRedNegative = 8,
    PercentNoDecimals = 9,
    PercentTwoDecimals = 10,
    Scientific = 11,
    FractionOneDigit = 12,
    FractionTwoDigit = 13,
    DateShort = 14,           // mm-dd-yy
    DateDayMonthShortYear = 15, // d-mmm-yy
    DateDayMonth = 16,        // d-mmm
    DateMonthYear = 17,       // mmm-yy
    Time12Hour = 18,
    Time12HourWithSeconds = 19,
    Time24Hour = 20,
    Time24HourWithSeconds = 21,
    DateTime = 22,
    AccountingNoCurrencySymbolNoDecimals = 37,
    AccountingNoCurrencySymbolNoDecimalsRedNegative = 38,
    AccountingNoCurrencySymbolTwoDecimals = 39,
    AccountingNoCurrencySymbolTwoDecimalsRedNegative = 40,
    AccountingNoDecimals = 41,
    AccountingNoDecimalsRedNegative = 42,
    AccountingTwoDecimals = 43,
    AccountingTwoDecimalsRedNegative = 44,
    TimeMinutesSeconds = 45,
    TimeElapsed = 46,
    TimeMinutesSecondsTenths = 47,
    ScientificCompact = 48,
    Text = 49,

    // CUSTOM formats >= 164
    DateSortable = 164,
    DateTimeSortable = 165,
    DateDayOfWeek = 166,
    DateDayOfWeekShort = 167,
    DateYearMonth = 168,
    DateMonthDay = 169,
}

/*

TODO:

| ID | Format Code                                                     | Description                          |
| -- | --------------------------------------------------------------- | ------------------------------------ |
| 0  | `General`                                                       | General format                       |
| 1  | `0`                                                             | Integer                              |
| 2  | `0.00`                                                          | 2 decimal places                     |
| 3  | `#,##0`                                                         | Thousands separator                  |
| 4  | `#,##0.00`                                                      | Thousands + 2 decimals               |
| 5  | `$#,##0_);($#,##0)`                                             | Currency, no decimals                |
| 6  | `$#,##0_);[Red]($#,##0)`                                        | Currency, no decimals, red neg.      |
| 7  | `$#,##0.00_);($#,##0.00)`                                       | Currency with 2 decimals             |
| 8  | `$#,##0.00_);[Red]($#,##0.00)`                                  | Currency + 2 decimals, red neg.      |
| 9  | `0%`                                                            | Percent, no decimals                 |
| 10 | `0.00%`                                                         | Percent, 2 decimals                  |
| 11 | `0.00E+00`                                                      | Scientific notation                  |
| 12 | `# ?/?`                                                         | Fraction (1-digit)                   |
| 13 | `# ??/??`                                                       | Fraction (2-digit)                   |
| 14 | `mm-dd-yy`                                                      | Date (U.S. style)                    |
| 15 | `d-mmm-yy`                                                      | Date (e.g. 1-Mar-22)                 |
| 16 | `d-mmm`                                                         | Date (day + short month)             |
| 17 | `mmm-yy`                                                        | Date (month + year)                  |
| 18 | `h:mm AM/PM`                                                    | Time 12-hour                         |
| 19 | `h:mm:ss AM/PM`                                                 | Time 12-hour w/ seconds              |
| 20 | `h:mm`                                                          | Time 24-hour                         |
| 21 | `h:mm:ss`                                                       | Time 24-hour w/ seconds              |
| 22 | `m/d/yy h:mm`                                                   | Date + time                          |
| 37 | `_(* #,##0_);_(* (#,##0);_(* "-"_);_(@_)`                       | Accounting, no decimals              |
| 38 | `_(* #,##0_);_(* [Red](#,##0);_(* "-"_);_(@_)`                  | Accounting, red negatives            |
| 39 | `_(* #,##0.00_);_(* (#,##0.00);_(* "-"??_);_(@_)`               | Accounting, 2 decimals               |
| 40 | `_(* #,##0.00_);_(* [Red](#,##0.00);_(* "-"??_);_(@_)`          | Accounting, 2 decimals + red         |
| 41 | `_("$"* #,##0_);_("$"* (#,##0);_("$"* "-"_);_(@_)`              | Accounting (currency symbol)         |
| 42 | `_("$"* #,##0_);_("$"* [Red](#,##0);_("$"* "-"_);_(@_)`         | Accounting (symbol + red)            |
| 43 | `_("$"* #,##0.00_);_("$"* (#,##0.00);_("$"* "-"??_);_(@_)`      | Accounting (symbol, 2 dec.)          |
| 44 | `_("$"* #,##0.00_);_("$"* [Red](#,##0.00);_("$"* "-"??_);_(@_)` | Accounting (symbol + red + decimals) |
| 45 | `mm:ss`                                                         | Time (minutes + seconds)             |
| 46 | `[h]:mm:ss`                                                     | Elapsed time                         |
| 47 | `mmss.0`                                                        | Minute-second.tenths                 |
| 48 | `##0.0E+0`                                                      | Scientific notation                  |
| 49 | `@`                                                             | Text                                 |

*/
