using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BorgWin10WPF.Puzzles;



namespace BorgWin10WPF.Save
{
    public static class SaveLoader
    {
        public static bool SaveFileExistsYN(string FileName)
        {
            if (File.Exists(Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Assets", FileName)))
                return true;
            return false;
        }
        public static async Task<List<SaveDefinition>> LoadSavesFromAsset(string FileName)
        {
            List<SaveDefinition> defs = new List<SaveDefinition>();

            string[] lines = null;// linesList.ToArray();
            try
            {
                lines = File.ReadAllLines(Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Assets", FileName));
            }
            catch
            {
                lines = new string[0];
            }
            if (lines.Length > 0)
            {
                for (var i = 0; i < lines.Length; i++)
                {
                    string[] linevals = lines[i].Split(new char[] { ',' }, StringSplitOptions.None);
                    if (linevals.Length > 13)
                    {
                        var savedef = new SaveDefinition()
                        {
                            SaveRowType = linevals[0],
                            SaveName = linevals[1],
                            SaveScene = linevals[2],
                            SaveSceneInt = Convert.ToInt32(linevals[3]),
                            SaveFrame = Convert.ToInt32(linevals[4]),
                            DoNothingCount = Convert.ToInt32(linevals[5]),
                            VisitedBorgifiedScenev_12 = Convert.ToInt32(linevals[6]),
                            Chapter_V_16_ComputerCoreClicks = Convert.ToInt32(linevals[7]),
                            Chapter_V_17_ComputerCoreClicks = Convert.ToInt32(linevals[8]),
                            Chapter_V_18_CircuitClicks = Convert.ToInt32(linevals[9]),
                            BorgIPCount = Convert.ToInt32(linevals[10]),
                            GridID = Convert.ToInt32(linevals[11]),
                            Volume = Convert.ToInt32(linevals[12])
                        };
                        int puzzlestates = Convert.ToInt32(linevals[13]);
                        if (puzzlestates > 0 && puzzlestates < 8)
                        {
                            for (int statei=0;statei<puzzlestates;statei++)
                            {
                                SpecialPuzzleSaveState puzzleState = new SpecialPuzzleSaveState();
                                puzzleState.puzzlename = linevals[13 + (statei * 7) + 1];
                                puzzleState.int0 = Convert.ToInt32(linevals[13 + (statei * 7) + 2]);
                                puzzleState.int1 = Convert.ToInt32(linevals[13 + (statei * 7) + 3]);
                                puzzleState.int2 = Convert.ToInt32(linevals[13 + (statei * 7) + 4]);
                                puzzleState.str0 = linevals[13 + (statei * 7) + 5];
                                puzzleState.str1 = linevals[13 + (statei * 7) + 6];
                                puzzleState.str2 = linevals[13 + (statei * 7) + 7];
                                savedef.puzzlestate.Add(puzzleState);
                            }
                        }
                        defs.Add(savedef);
                    }
                }
            }
            return defs;
        }

        internal static async Task SaveSavesToAsset(List<SaveDefinition> saves, string FileName)
        {

            StringBuilder saveSB = new StringBuilder();
            foreach (var save in saves)
            {
                saveSB.Append(
                   string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13}",
                    save.SaveRowType,
                    save.SaveName,
                    save.SaveScene,
                    save.SaveSceneInt,
                    save.SaveFrame,
                    save.DoNothingCount,
                    save.VisitedBorgifiedScenev_12, 
                    save.Chapter_V_16_ComputerCoreClicks,
                    save.Chapter_V_17_ComputerCoreClicks,
                    save.Chapter_V_18_CircuitClicks,
                    save.BorgIPCount,
                    save.GridID, 
                    save.Volume,
                    save.puzzlestate.Count));

                foreach (var puzzlestate in save.puzzlestate)
                {
                    saveSB.Append($",{puzzlestate.State2Line()}");
                }
                saveSB.AppendLine("");

            }

            System.Diagnostics.Debug.WriteLine(string.Format("Saved to: {0}", Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Assets", FileName)));

            File.WriteAllText(Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Assets", FileName), saveSB.ToString());
        }
    }
}
