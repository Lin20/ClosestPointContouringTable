using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClosestPointContouringTable
{
	public class Program
	{
		static int[] DX = { 0, 1, 0, 1, 0, 1, 0, 1 };
		static int[] DY = { 0, 0, 1, 1, 0, 0, 1, 1 };
		static int[] DZ = { 0, 0, 0, 0, 1, 1, 1, 1 };

		static int[] tri_case_0 = { 2, 1, 7 };
		static int[] tri_case_1 = { 2, 1, 7, 2, 0, 1 };
		static int[] tri_case_2 = { 2, 0, 1, 2, 1, 7, 1, 5, 7 };
		static int[] tri_case_3 = { 2, 0, 1, 2, 1, 7, 1, 5, 7, 7, 6, 5 };
		static int[] tri_case_4 = { 7, 6, 0, 0, 1, 7 };
		static int[] tri_case_5 = { 7, 6, 0, 0, 1, 7, 1, 3, 7, 7, 5, 1 };
		static int[] tri_case_6 = { 2, 1, 7, 7, 1, 4, 2, 4, 1, 2, 7, 4 };
		static int[] tri_case_7 = { 2, 4, 1, 2, 1, 3, 3, 1, 7, 7, 4, 1, 3, 7, 2, 2, 7, 4 };

		static int[] tri_quad_x = { 0, 4, 6, 6, 2, 0 };
		static int[] tri_quad_y = { 0, 1, 5, 5, 4, 0 };
		static int[] tri_quad_z = { 0, 2, 3, 3, 1, 0 };

		static int[][] tri_cases = { tri_case_0, tri_case_1, tri_case_2, tri_case_3, tri_case_4, tri_case_5, tri_case_6, tri_case_7, tri_quad_x, tri_quad_y, tri_quad_z };

		// These are ignored. According to the paper, only the 8 cases and quads are used, even though the corner and "unknown" masks come up quite a bit...
		static int[] tri_corner = { 0, 1, 2 };
		static int[] tri_unknown_0 = { 2, 1, 4 };
		static int[] tri_unknown_1 = { 0, 3, 4 };
		static int[] tri_unknown_2 = { 1, 0, 3, 3, 0, 4 };
		static int[] tri_unknown_3 = { };
		static int[] tri_unknown_4 = { };

		static int mask_case_0 = 0b10000110;
		static int mask_case_1 = 0b10000111;
		static int mask_case_2 = 0b10100111;
		static int mask_case_3 = 0b11100111;
		static int mask_case_4 = 0b11000011;
		static int mask_case_5 = 0b11101011;
		static int mask_case_6 = 0b10010110;
		static int mask_case_7 = 0b10011110;
		static int mask_x_quad = 0b01010101; // x quad
		static int mask_y_quad = 0b00110011; // y quad
		static int mask_z_quad = 0b11110000; // z quad
		static int mask_corner = 0b00000111; // corner
		static int mask_unknown_0 = 0b00010111; // unknown
		static int mask_unknown_1 = 0b00011001; // unknown
		static int mask_unknown_2 = 0b00011011; // unknown
		static int mask_unknown_3 = 0b00011111; // unknown
		static int mask_unknown_4 = 0b000111111; // unknown

		static int[] mask_cases = { mask_case_0, mask_case_1, mask_case_2, mask_case_3, mask_case_4, mask_case_5, mask_case_6, mask_case_7, mask_x_quad, mask_y_quad, mask_z_quad, mask_corner, mask_unknown_0, mask_unknown_1, mask_unknown_2, mask_unknown_3, mask_unknown_4 };

		static int[] rot_bits_x = { 2, 3, 6, 7, 0, 1, 4, 5 };
		static int[] rot_bits_y = { 1, 5, 3, 7, 0, 4, 2, 6 };
		static int[] rot_bits_z = { 1, 3, 0, 2, 5, 7, 4, 6 };

		static int[] sym_bits_x = { 1, 0, 3, 2, 5, 4, 7, 6 };
		static int[] sym_bits_y = { 2, 3, 0, 1, 6, 7, 4, 5 };
		static int[] sym_bits_z = { 4, 5, 6, 7, 0, 1, 2, 3 };

		public static void Main(string[] args)
		{
			int[,] output = new int[256, 18];
			int failed_cases = 0;
			int unknown_cases = 0;

			StringBuilder str_out = new StringBuilder();

			for (int i = 0; i < 256; i++)
			{
				int num_bits = 0;
				int mask = i;
				for (int k = 0; k < 8; k++)
				{
					if ((mask & 1) != 0)
						num_bits++;
					mask >>= 1;
				}

				int n_tris = 0;
				switch (num_bits)
				{
					case 0:
					case 1:
					case 2:
					case 7:
					case 8:
						Console.WriteLine("{0}\t: skipping.", i, n_tris);
						for (int k = 0; k < 18; k++)
						{
							output[i, k] = -1;
						}
						break;

					default:
						bool flip;
						int[] counts;
						int out_mask = identify_case(i, out flip, out counts);
						if (out_mask == -1)
						{
							Console.WriteLine("{0}\t: Error! Unidentified case!", i);
							failed_cases++;
						}
						else
						{
							if (out_mask > 10)
							{
								unknown_cases++;
								for (int k = 0; k < 18; k++)
								{
									output[i, k] = -1;
								}
							}
							else
							{
								write_output(output, i, out_mask, counts, flip);
							}
							Console.WriteLine("{0}\t: case {1}{2}.", i, out_mask, (out_mask > 11 ? " (unknown)" : out_mask == 11 ? " (corner)" : out_mask >= 8 ? " (quad)" : ""));
						}
						break;

				}
			}

			str_out.AppendLine("{");
			for (int i = 0; i < 256; i++)
			{
				str_out.Append("\t{ ");
				for (int k = 0; k < 18; k++)
				{
					str_out.Append(output[i, k].ToString() + (k < 17 ? ", " : " }"));
				}
				if (i < 255)
					str_out.AppendLine(",");
				else
					str_out.AppendLine();
			}
			str_out.AppendLine("}");

			Console.WriteLine("\nComplete with {0} cases successfully completed ({1} failed, {2} unknowns).", 256 - failed_cases, failed_cases, unknown_cases);


			try
			{
				StreamWriter sw = new StreamWriter(File.Open("cpc_table.txt", FileMode.Create));
				sw.Write(str_out.ToString());
				sw.Close();
			}
			catch (Exception)
			{
				Console.WriteLine("Failed to write file.");
			}

			Console.ReadKey();
		}

		static int identify_case(int mask, out bool flip, out int[] counts)
		{
			bool sym = false;
			flip = false;

			for (int i = 0; i < mask_cases.Length; i++)
			{
				for (int x = 0; x < 4; x++)
				{
					for (int y = 0; y < 4; y++)
					{
						for (int z = 0; z < 4; z++)
						{
							for (int sx = 0; sx < 2; sx++)
							{
								for (int sy = 0; sy < 2; sy++)
								{
									for (int sz = 0; sz < 2; sz++)
									{
										int rot_mask = mask_cases[i];
										for (int sub = 0; sub < x; sub++)
										{
											rot_mask = rotate_mask(rot_mask, rot_bits_x);
										}
										for (int sub = 0; sub < y; sub++)
										{
											rot_mask = rotate_mask(rot_mask, rot_bits_y);
										}
										for (int sub = 0; sub < z; sub++)
										{
											rot_mask = rotate_mask(rot_mask, rot_bits_z);
										}

										if (sx == 1)
											rot_mask = rotate_mask(rot_mask, sym_bits_x);
										if (sy == 1)
											rot_mask = rotate_mask(rot_mask, sym_bits_y);
										if (sz == 1)
											rot_mask = rotate_mask(rot_mask, sym_bits_z);
										sym = sx == 1 || sy == 1 || sz == 1;
										flip = sym;
										if (rot_mask == mask)
										{
											counts = new int[] { x, y, z, sx, sy, sz };
											return i;
										}
									}
								}
							}
						}
					}
				}
			}

			counts = new int[] { 0, 0, 0, 0, 0, 0 };
			return -1;
		}

		static int rotate_mask(int mask, int[] rot_bits)
		{
			int[] bits = new int[8];
			for (int i = 0; i < 8; i++)
			{
				bits[i] = mask & 1;
				mask >>= 1;
			}

			int[] new_bits = new int[8];
			int out_mask = 0;
			for (int i = 0; i < 8; i++)
			{
				new_bits[i] = bits[rot_bits[i]];
				out_mask |= bits[rot_bits[i]] << i;
			}

			Debug.Assert(out_mask >= 0 && out_mask <= 255);
			return out_mask;
		}

		static int[] rotate_indexes(int[] indexes, int[] rot_bits)
		{
			int[] output = new int[8];
			for (int i = 0; i < 8; i++)
			{
				output[i] = indexes[rot_bits[i]];
			}

			return output;
		}

		static void write_output(int[,] output, int mask, int case_id, int[] counts, bool flip)
		{
			int[] new_indexes = new int[8];

			for (int x = 0; x <= counts[0]; x++)
			{
				for (int y = 0; y <= counts[1]; y++)
				{
					for (int z = 0; z <= counts[2]; z++)
					{
						for (int sx = 0; sx <= counts[3]; sx++)
						{
							for (int sy = 0; sy <= counts[4]; sy++)
							{
								for (int sz = 0; sz <= counts[5]; sz++)
								{
									for (int i = 0; i < 8; i++)
									{
										new_indexes[i] = i;
									}
									int rot_mask = mask_cases[case_id];
									for (int sub = 0; sub < x; sub++)
									{
										new_indexes = rotate_indexes(new_indexes, rot_bits_x);
									}
									for (int sub = 0; sub < y; sub++)
									{
										new_indexes = rotate_indexes(new_indexes, rot_bits_y);
									}
									for (int sub = 0; sub < z; sub++)
									{
										new_indexes = rotate_indexes(new_indexes, rot_bits_z);
									}

									if (sx == 1)
										new_indexes = rotate_indexes(new_indexes, sym_bits_x);
									if (sy == 1)
										new_indexes = rotate_indexes(new_indexes, sym_bits_y);
									if (sz == 1)
										new_indexes = rotate_indexes(new_indexes, sym_bits_z);
								}
							}
						}
					}
				}
			}

			for (int i = 0; i < 18; i++)
			{
				if (i >= tri_cases[case_id].Length)
					output[mask, i] = -1;
				else
				{
					if (flip)
						output[mask, i] = new_indexes[tri_cases[case_id][2 - i % 3 + i / 3 * 3]];
					else
						output[mask, i] = new_indexes[tri_cases[case_id][i]];
				}
			}
		}
	}
}
