using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Data
{
    public class PlayerNoviceGuidanceData
    {
        private int basicOperation;//0 - No  1 - Completed
        private int buildTraining;//0 - No  1 - Completed
        private int isSkipGuide;//0 - No Skip  1 - Skip
        private int npcTraining;//0 - No  1 - Completed
        private int skillTraining;//0 - No  1 - Completed
        private int trainingMode;//0 - No  1 - Completed

        public void SetBasicOperation( int basicGuide )
        {
            basicOperation = basicGuide;
        }

        public int GetBasicOperation()
        {
            return basicOperation;
        }

        public void SetBuildTraining( int buildGuide )
        {
            buildTraining = buildGuide;
        }

        public int GetBuildTraining()
        {
            return buildTraining;
        }

        public void SetIsSkipGuide( int isSkip )
        {
            isSkipGuide = isSkip;
        }

        public int GetIsSkipGuide()
        {
            return isSkipGuide;
        }

        public void SetNpcTraining( int npcGuide )
        {
            npcTraining = npcGuide;
        }

        public int GetNpcTraining()
        {
            return npcTraining;
        }

        public void SetSkillTraining( int skillGuide )
        {
            skillTraining = skillGuide;
        }

        public int GetSkillTraining()
        {
            return skillTraining;
        }

        public void SetTrainingMode( int trainingGuide )
        {
            trainingMode = trainingGuide;
        }

        public int GetTrainingMode()
        {
            return trainingMode;
        }
    }
}
