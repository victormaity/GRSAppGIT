using GlobalReportingSystem.Core.Models.Entities;
using GlobalReportingSystem.Data.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalReportingSystem.DataLINQ
{
    public class HelperConfigurationProvider
    {
        GRSDataBaseEntities DBENT = new GRSDataBaseEntities();

        #region Team

        public List<TeamInfo> GetTeamInformation(int currentProjectId, int userId)
        {
            return (from x in DBENT.TeamInfoes where x.ParentProject == currentProjectId select x).ToList();
        }

        public TeamInfo GetTeamInformationById(Int64 teamId)
        {
            return (from x in DBENT.TeamInfoes where x.ID == teamId select x).FirstOrDefault();
        }

        public string AddNewTeam(string teamName, string comment, int curretnProjectId, int userId)
        {
            try
            {
                var userdata = (from x in DBENT.Users where x.ID == userId select x).FirstOrDefault();
                if (userdata != null)
                {
                    if (userdata.UserGlobalAdmin || userdata.UserAdmin)
                    {
                        if ((!string.IsNullOrEmpty(teamName)) && (!string.IsNullOrWhiteSpace(teamName)))
                        {
                            teamName = teamName.Trim();
                            bool isTeamExist = (from x in DBENT.TeamInfoes where x.TeamName == teamName && x.ParentProject == curretnProjectId select x).Any();
                            if (isTeamExist == false)
                            {
                                TeamInfo entTeam = new TeamInfo()
                                {
                                    TeamName = teamName,
                                    Comment = comment,
                                    ParentProject = curretnProjectId
                                };
                                DBENT.TeamInfoes.Add(entTeam);
                                DBENT.SaveChanges();
                                return "Team '" + teamName + "' added successfully.";
                            }
                            else
                            {
                                return "Team name '" + teamName + "' already exist.";
                            }
                        }
                        else
                        {
                            return "Invalid team name!";
                        }
                    }
                    else
                    {
                        return "You do not have permission to add/update/delete team information. Please contact your user administrator or global administrator.";
                    }
                }
                else
                {
                    return "Invalid user!";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string UpdateExistingTeam(Int64 teamId, string teamName, string comment, int currentProjectId, int userId)
        {
            try
            {
                var userdata = (from x in DBENT.Users where x.ID == userId select x).FirstOrDefault();
                if (userdata != null)
                {
                    if (userdata.UserGlobalAdmin || userdata.UserAdmin)
                    {
                        bool isTeamnameExist = (from x in DBENT.TeamInfoes where x.TeamName == teamName && x.ParentProject == currentProjectId && x.ID != teamId select x).Any();
                        if (isTeamnameExist == false)
                        {
                            var teamdata = (from x in DBENT.TeamInfoes where x.ID == teamId && x.ParentProject == currentProjectId select x).FirstOrDefault();
                            if (teamdata != null)
                            {
                                teamdata.TeamName = teamName;
                                teamdata.Comment = comment;
                                DBENT.SaveChanges();
                                return "Team record updated successfully.";
                            }
                            else
                            {
                                return "Team record does not exist!";
                            }
                        }
                        else
                        {
                            return "Team name already exist.";
                        }
                    }
                    else
                    {
                        return "You do not have permission to add/update/delete team information. Please contact your user administrator or global administrator.";
                    }
                }
                else
                {
                    return "Invalid user!";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string DeleteExistingTeam(Int64 teamId, int currentProjectId, int userId)
        {
            try
            {
                var userdata = (from x in DBENT.Users where x.ID == userId select x).FirstOrDefault();
                if (userdata != null)
                {
                    if (userdata.UserGlobalAdmin || userdata.UserAdmin)
                    {
                        var delTeam = (from x in DBENT.TeamInfoes where x.ID == teamId && x.ParentProject == currentProjectId select x).FirstOrDefault();
                        if (delTeam != null)
                        {
                            DBENT.TeamInfoes.Remove(delTeam);
                            DBENT.SaveChanges();
                            return "Team deleted!";
                        }
                        else
                        {
                            return "Team not found.";
                        }
                    }
                    else
                    {
                        return "You do not have permission to add/update/delete team information. Please contact your user administrator or global administrator.";
                    }
                }
                else
                {
                    return "Invalid user!";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        #endregion Team

        #region Release

        public List<ReleaseInfo> GetReleaseInformation(int currentProjectId, int userId)
        {
            return (from x in DBENT.ReleaseInfoes where x.ParentProjectId == currentProjectId select x).ToList();
        }

        public ReleaseInfo GetReleaseInformationById(Int64 releaseId)
        {
            return (from x in DBENT.ReleaseInfoes where x.ID == releaseId select x).FirstOrDefault();
        }

        public string AddNewRelease(string releaseName, string releaseDate, string comment, int curretnProjectId, int userId)
        {
            DateTime convertedReleaseDate = DateParseMM_DD_YYYY(releaseDate);
            try
            {
                var userdata = (from x in DBENT.Users where x.ID == userId select x).FirstOrDefault();
                if (userdata != null)
                {
                    if (userdata.UserGlobalAdmin || userdata.UserAdmin)
                    {
                        if ((!string.IsNullOrEmpty(releaseName)) && (!string.IsNullOrWhiteSpace(releaseName)))
                        {
                            releaseName = releaseName.Trim();
                            bool isReleaseExist = (from x in DBENT.ReleaseInfoes where x.ReleaseName == releaseName && x.ParentProjectId == curretnProjectId select x).Any();
                            if (isReleaseExist == false)
                            {
                                ReleaseInfo entRelease = new ReleaseInfo()
                                {
                                    ReleaseName = releaseName,
                                    ReleaseDate = convertedReleaseDate,
                                    Comment = comment,
                                    ParentProjectId = curretnProjectId
                                };
                                DBENT.ReleaseInfoes.Add(entRelease);
                                DBENT.SaveChanges();
                                return "Release '" + releaseName + "' added successfully.";
                            }
                            else
                            {
                                return "Release name '" + releaseName + "' already exist.";
                            }
                        }
                        else
                        {
                            return "Invalid release name!";
                        }
                    }
                    else
                    {
                        return "You do not have permission to add/update/delete release information. Please contact your user administrator or global administrator.";
                    }
                }
                else
                {
                    return "Invalid user!";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string UpdateExistingRelease(Int64 releaseId, string releaseName, string releaseDate, string comment, int currentProjectId, int userId)
        {
            DateTime convertedReleaseDate = DateParseMM_DD_YYYY(releaseDate);
            try
            {
                var userdata = (from x in DBENT.Users where x.ID == userId select x).FirstOrDefault();
                if (userdata != null)
                {
                    if (userdata.UserGlobalAdmin || userdata.UserAdmin)
                    {
                        bool isReleaseNameExist = (from x in DBENT.ReleaseInfoes where x.ReleaseName == releaseName && x.ParentProjectId == currentProjectId && x.ID != releaseId select x).Any();
                        if (isReleaseNameExist == false)
                        {
                            var releasedata = (from x in DBENT.ReleaseInfoes where x.ID == releaseId && x.ParentProjectId == currentProjectId select x).FirstOrDefault();
                            if (releasedata != null)
                            {
                                releasedata.ReleaseName = releaseName;
                                releasedata.ReleaseDate = convertedReleaseDate;
                                releasedata.Comment = comment;
                                DBENT.SaveChanges();
                                return "Release record updated successfully.";
                            }
                            else
                            {
                                return "Release record does not exist!";
                            }
                        }
                        else
                        {
                            return "Release name already exist.";
                        }
                    }
                    else
                    {
                        return "You do not have permission to add/update/delete release information. Please contact your user administrator or global administrator.";
                    }
                }
                else
                {
                    return "Invalid user!";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string DeleteExistingRelease(Int64 releaseId, int currentProjectId, int userId)
        {
            try
            {
                var userdata = (from x in DBENT.Users where x.ID == userId select x).FirstOrDefault();
                if (userdata != null)
                {
                    if (userdata.UserGlobalAdmin || userdata.UserAdmin)
                    {
                        var delRelease = (from x in DBENT.ReleaseInfoes where x.ID == releaseId && x.ParentProjectId == currentProjectId select x).FirstOrDefault();
                        if (delRelease != null)
                        {
                            DBENT.ReleaseInfoes.Remove(delRelease);
                            DBENT.SaveChanges();
                            return "Release informaiton deleted!";
                        }
                        else
                        {
                            return "Release information not found.";
                        }
                    }
                    else
                    {
                        return "You do not have permission to add/update/delete release information. Please contact your user administrator or global administrator.";
                    }
                }
                else
                {
                    return "Invalid user!";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        #endregion Release

        #region GENERAL
        public DateTime DateParseMM_DD_YYYY(string inputDateString)
        {
            if (inputDateString.Length == 10)
            {
                //inputdatetime format is MM DD YYYY

                string monthdata = inputDateString.Substring(0, 2);
                int monthconverted = 0;
                bool ismonthvalid = Int32.TryParse(monthdata, out monthconverted);

                string datedata = inputDateString.Substring(3, 2);
                int dateconverted = 0;
                bool isDatevalid = Int32.TryParse(datedata, out dateconverted);

                string yeardata = inputDateString.Substring(6);
                int yearconverted = 0;
                bool isyearvalid = Int32.TryParse(yeardata, out yearconverted);
                return new DateTime(yearconverted, monthconverted, dateconverted, 0, 0, 0);
            }
            else
            {
                return DateTime.Now;
            }
        }
        #endregion GENERAL
    }
}
