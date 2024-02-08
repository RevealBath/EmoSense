Please refer to the accompanying CHI paper.

This project was built for Unity (2021.3.0f1). Compatible with the Vive Pro Eye and supports the following physiological sensors and devices: 

- Polar H10 HR strap
- Shimmer GSR+
- Vive Pro Eye Facial Tracker
- Wahoo KICKR smart bike

The scene used for the Study is in Assets/Scenes/Updated/Testing_Scene.

In the GameObject Calibration_Loader -> Calibration Data Loader Script - This script loads a users base line measures for cleaning physiological data and scaling the exercise intensity. 
This will need a path to a valid calibration data file (csv path). Example calibration data files are provided in Assets/CSV_Data/Example_Calibration_Data. Set csv path to one of these files if just want to see the study procedure/virtual environments.

When running the scene: 
- In the GameObject StudyManager -> StudyManager Script - You can configure the VE condition order, the Exercise Intensity, and the emotion virtual environment exposure scene time.
- Press 'T' to remove scene eye test.
- In the GameObject StudyManager -> StudyManager Script - Ensure 'Record Data' is ticked.

Starting the Procedure:
- Press 'Space' to begin Warmup.
- In the GameObject StudyManager/WarmupManager -> WarmUpManager Script you can see when the target HR has been achieved, or can tick 'Overide Target HR'.
- Press 'Space' to end Warmup.
- Exposure Scene will run for specified duration.
- Cooldown will begin. User responses to affect ground truth questions can be recorded in StudyManager/CoolDownManager -> CoolDownManager Script -> Participant Answer. Once entered, tick 'Participant Input Ready'.
- Once all answers are recorded, press 'Space' to begin the next warmup.
  
The above repeats until all emotion environments have been completed. Data Files are stored in Assets/CSV_Data.
