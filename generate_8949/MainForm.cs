﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace generate_8949
{
	public class LedgerEntry
	{
		public string Security;
		public DateTime Day;
		public Decimal UnitQty;
		public Decimal UnitPrice;
	}

	public class CapGainEntry
	{
		public string Security;
		public DateTime AcqDate;
		public DateTime SaleDate;
		public Decimal UnitQty;
		public Decimal UnitBuyPrice;
		public Decimal UnitSellPrice;
		public Decimal CostBasis;
		public Decimal Proceeds;
		public Decimal CapitalGain;
	}

	public partial class MainForm : Form
	{
		public MainForm()
		{
			InitializeComponent();
		}

		private void StartCalc_Click(object sender, EventArgs e)
		{
			if (!File.Exists(SourceDataFileName.Text))
			{
				MessageBox.Show("Input data file doesn't exist!", "Can't open file", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			else
			{
				try
				{
					List<string> SourceData = new List<string>();

					using (StreamReader sr = new StreamReader(new FileStream(SourceDataFileName.Text, FileMode.Open)))
					{
						while (!sr.EndOfStream)
						{
							SourceData.Add(sr.ReadLine());
						}
					}

					FileInfo infile = new FileInfo(SourceDataFileName.Text);

					ProcessDataset(infile.Name.Substring(0, infile.Name.Length - infile.Extension.Length), SourceData.ToArray());
				}
				catch (Exception ex)
				{
					MessageBox.Show("Exception during calculation:\n\n" + ex.Message, "Calc Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		private void SelectFileButton_Click(object sender, EventArgs e)
		{
			OpenFileDialog ofd = new OpenFileDialog();

			if (ofd.ShowDialog() == DialogResult.OK)
			{
				SourceDataFileName.Text = ofd.FileName;
			}
		}

		private void ProcessDataset(string filename, string[] DataSet)
		{
			List<LedgerEntry> Buys = new List<LedgerEntry>();
			List<LedgerEntry> Sells = new List<LedgerEntry>();

			// process data set into a list of buys and sells
			foreach (string row in DataSet)
			{
				string[] cells = row.Split(',');

				LedgerEntry entry = new LedgerEntry();

				if (cells.Length > 3 &&
					DateTime.TryParseExact(cells[1], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out entry.Day) &&
					Decimal.TryParse(cells[2], out entry.UnitQty) &&
					Decimal.TryParse(cells[3], out entry.UnitPrice))
				{
					entry.Security = cells[0];

					entry.UnitQty = Decimal.Round(entry.UnitQty, 8, MidpointRounding.AwayFromZero);
					entry.UnitPrice = Decimal.Round(entry.UnitPrice, 4, MidpointRounding.AwayFromZero);

					if (entry.UnitQty < 0)
					{
						entry.UnitQty *= -1;
						Sells.Add(entry);
					}
					else
					{
						Buys.Add(entry);
					}
				}
			}

			// sort the orders
			Buys.Sort((x, y) => x.Day.CompareTo(y.Day));
			Sells.Sort((x, y) => x.Day.CompareTo(y.Day));

			List<CapGainEntry> CapitalGains = new List<CapGainEntry>();

			foreach (LedgerEntry sale in Sells)
			{
				foreach (LedgerEntry buy in Buys)
				{
					//if not the same security or nothing left, skip it
					if (buy.UnitQty <= 0 || sale.Security != buy.Security)
					{
						continue;
					}
					else
					{
						CapGainEntry entry = new CapGainEntry();

						entry.Security = sale.Security;
						entry.UnitBuyPrice = buy.UnitPrice;
						entry.AcqDate = buy.Day;

						entry.UnitSellPrice = sale.UnitPrice;
						entry.SaleDate = sale.Day;

						if (sale.UnitQty > buy.UnitQty)
						{
							sale.UnitQty -= buy.UnitQty;
							entry.UnitQty = buy.UnitQty;
							buy.UnitQty = 0;
						}
						else
						{
							entry.UnitQty = sale.UnitQty;
							buy.UnitQty -= sale.UnitQty;
							sale.UnitQty = 0;
						}

						entry.CostBasis = Decimal.Round(entry.UnitQty * entry.UnitBuyPrice, 4, MidpointRounding.AwayFromZero);
						entry.Proceeds = Decimal.Round(entry.UnitQty * entry.UnitSellPrice, 4, MidpointRounding.AwayFromZero);
						entry.CapitalGain = Decimal.Round(entry.Proceeds - entry.CostBasis, 4, MidpointRounding.AwayFromZero);

						CapitalGains.Add(entry);
					}
				}
			}

			List<CapGainEntry> ShortTerm = new List<CapGainEntry>();
			List<CapGainEntry> LongTerm = new List<CapGainEntry>();

			foreach (CapGainEntry ce in CapitalGains)
			{
				if (ce.UnitQty > 0)
				{
					if (ce.SaleDate.Year > ce.AcqDate.Year + 1 ||
										(ce.SaleDate.Year > ce.AcqDate.Year &&
										ce.SaleDate.Month >= ce.AcqDate.Month &&
										ce.SaleDate.Day >= ce.AcqDate.Day))
					{
						LongTerm.Add(ce);
					}
					else
					{
						ShortTerm.Add(ce);
					}
				}
			}

			//generate CSV file for long term capital gains
			if (LongTerm.Count > 0)
			{
				List<string> ltg = new List<string>();

				ltg.Add("DESCRIPTION OF PROPERTY,DATE ACQUIRED,DATE SOLD OR DISPOSED,PROCEEDS,COST OR OTHER BASIS,CODE(S) FROM INSTRUCTIONS,AMOUNT OF ADJUSTMENT,GAIN OR (LOSS)");

				foreach (CapGainEntry ce in LongTerm)
				{
					ltg.Add(CapGainRow(ce));
				}

				ltg.Add(",,,,,,,,");
				ltg.Add(GetTotalsRow(LongTerm));

				SaveTextFile("f8949_" + filename + "_longterm.csv", ltg.ToArray());
			}

			//generate CSV file for short term capital gains
			if (ShortTerm.Count > 0)
			{
				List<string> stg = new List<string>();

				stg.Add("DESCRIPTION OF PROPERTY,DATE ACQUIRED,DATE SOLD OR DISPOSED,PROCEEDS,COST OR OTHER BASIS,CODE(S) FROM INSTRUCTIONS,AMOUNT OF ADJUSTMENT,GAIN OR (LOSS)");

				foreach (CapGainEntry ce in ShortTerm)
				{
					stg.Add(CapGainRow(ce));
				}

				stg.Add(",,,,,,,,");
				stg.Add(GetTotalsRow(ShortTerm));

				SaveTextFile("f8949_" + filename + "_shortterm.csv", stg.ToArray());
			}

			//generate CSV file for remaining amounts of securities after all other calculations
			List<string> remainder = new List<string>();

			remainder.Add("DESCRIPTION OF PROPERTY,DATE ACQUIRED,QUANTITY,UNIT PRICE");

			foreach (LedgerEntry le in Buys)
			{
				if (le.UnitQty > 0)
				{
					remainder.Add(LedgerRow(le));
				}
			}
			SaveTextFile(filename + "_remainder.csv", remainder.ToArray());
		}

		private void SaveTextFile(string filename, string[] text)
		{
			SaveFileDialog sfd = new SaveFileDialog();

			sfd.FileName = filename;

			if (sfd.ShowDialog() == DialogResult.OK)
			{
				using (StreamWriter sw = new StreamWriter(new FileStream(sfd.FileName, FileMode.Create)))
				{
					foreach (string s in text)
					{
						sw.WriteLine(s);
					}
				}
			}
		}

		private string CapGainRow(CapGainEntry ce)
		{
			return
				ce.UnitQty.ToString() + " " + ce.Security + "," +
				DateToText(ce.AcqDate) + "," +
				DateToText(ce.SaleDate) + "," +
				ce.Proceeds.ToString() + "," +
				ce.CostBasis.ToString() + ",,," +
				(ce.CapitalGain >= 0 ? ce.CapitalGain.ToString() : "(" + (ce.CapitalGain * -1).ToString() + ")");
		}

		private string LedgerRow(LedgerEntry le)
		{
			return
				le.Security + "," +
				DateToText(le.Day) + "," +
				le.UnitQty.ToString() + "," +
				le.UnitPrice.ToString();
		}

		private string DateToText(DateTime dt)
		{
			return dt.Year.ToString("0000") + "-" + dt.Month.ToString("00") + "-" + dt.Day.ToString("00");
		}

		private string GetTotalsRow(List<CapGainEntry> entries)
		{
			Decimal TotalProceeds = 0;
			Decimal TotalCostBasis = 0;
			Decimal TotalGains = 0;

			foreach(CapGainEntry ce in entries)
			{
				TotalProceeds += ce.Proceeds;
				TotalCostBasis += ce.CostBasis;
				TotalGains += ce.CapitalGain;
			}

			if (TotalProceeds - TotalCostBasis != TotalGains)
			{
				throw new Exception();
			}

			return "TOTALS,,," + TotalProceeds.ToString() + "," + TotalCostBasis.ToString() + ",,," + TotalGains.ToString();
		}
	}
}
