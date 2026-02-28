
# DrawRec3D

A Unity3D tool and standalone model developed to support recognition of user-drawn 3D shapes. Useful for drawing/gesture-based AR/VR controls (e.g. controlling UIs with physical gestures, spawning 3D assets that matches a user's drawing), and general object detection. One of the focuses is ease of use - the included model is trained under a Siamese architecture for generalized recognition. This means the model is ready to use for new general 3D shapes and drawings. That being said, scripts to retrain the model with your own data are provided, along with an AR application to draw with controllers and save created drawings.

Below is a short demo video for another project called PicoTown, a mixed reality city-builder with a mechanic that utilizes DrawRec3D to match and transform user-created drawings to building assets.

## Setup

1. Install into a Unity project via UPM from the repository URL:

`https://github.com/gilbertdyer2/DrawRec3D_UPM`
*(Window->Package Manager->Click upper left '+' icon->Install package from git URL)*

2. Next, under `Assets/StreamingAssets`, create the directory path `DrawRec3D/RuntimeDrawings`. This is where you will insert drawings (represented by JSON files) to compare to during runtime.

3. Insert the DrawRec3D prefab into a scene. 

4. To query DrawRec3D for a match, obtain a `List<Vector3>` of points representing your drawing and call `DrawingRecognizer.GetMatch(points)` on the prefab's DrawingRecognizer component. This will find the closest match within `RuntimeDrawings/` and return a string of its respective filename.
## Tool Setup (AR Data Capture Application and Training Loop)

### Data Capture Setup
The data capture application allows you to draw with AR controllers and save custom drawings to the headset.

To run the application, you'll need a Meta Quest device with AR capabilities. A built apk file has been provided in the Unity Project's build folder, which can be loaded directly to a headset via SideQuest, or by building and running the Unity project with the headset connected.

Once launched, follow the instructions in the app to begin saving custom drawings.


Drawings are represented by a JSON format (example below)
```json
{
    "drawingName": "cube",
    "size": 177,
    "points": [
        {
            "x": -0.9423178434371948,
            "y": 0.1703234314918518,
            "z": -0.958172082901001
        },
        {
            "x": -0.9438960552215576,
            "y": 0.17107324302196504,
            "z": -0.9592971801757813
        },
	    ...
```

The AR tool will generate these for drawings created and saved within the tool. If you want to port over external drawings or models in different formats, the current solution requires you to convert each to a list of 3D points, then generate a JSON file with utils/JSONGeneration/PointToJSON.py

For further reference, `SaveDrawingToFile.cs` outlines the creation of a JSON file from a list of points.

### Training Loop Setup

1. This project uses a Conda environment to manage dependencies. First, within the root directory, create the environment: `conda env create -f environment.yml`, 

2. Then activate it: `conda activate DrawRec3D_TF210`

3. If using custom drawings, organize the json files into named subfolders and split these between the training and validation folders under dataset_RAW.

4. After this, run `util/raw_to_dataset.py` to generate an intermediary dataset with preprocessing steps.

5. Next, run all cells in `model.ipynb`. Make sure your environment is active.

6. Once complete, run `util/tf_to_onnx.py` to convert the model to an ONNX file for use in Unity. This can be put into your project's assets folder and dragged into a DrawRec3D prefab's "Model Asset" serialized field.

### Uploading Files to the Application
There may be cases where you want to upload created json files to the headset (e.g. adding to a dataset not already stored in the headset, or simply for viewing/testing)

Currently, to upload json files from desktop to Quest devices, you must first manually add the json files to the Unity project's Assets/StreamingAssets/DefaultDrawings directory, then rebuild (the README within the DefaultDrawings folder outlines how to arrange these files). After launching the build, you should see the new files.


This workflow is being improved. The largest hurdle is scoped storage on the Meta Quest (JSON drawings are stored within the app's persistent data path). Once SAF is implemented, this should allow the app to save and load drawings to a folder within /sdcard/downloads/ (which adb has permissions to upload files to) without needing to rebuild.

#### Pulling Files From the Headset
To pull saved drawings from the headset with adb, first locate the app's folder on the headset, then navigate to `files/DrawRec3D/gestureData`. The full path will look something like this:
`/storage/emulated/0/Android/data/com.UnityTechnologies.com.unity.template.urpblank/files/DrawRec3D/gestureData/`

Then, pull with adb
e.g.
`adb -s [DEVICE_SERIAL_NUMBER] pull /storage/emulated/0/Android/data/com.UnityTechnologies.com.unity.template.urpblank/files/DrawRec3D/gestureData/ [DESKTOP_FILEPATH_LOCATION]`

## Notes on Limitations

The included model may not perform well for very detailed objects or collections of drawings that are extremely similar, as everything fed into the model is reduced to 128 points. It's worth looking into alternatives if your data requires dense and/or detailed point-cloud representations.

This project is still in a somewhat early stage, and improvements to the training data are being made. 
In addition, the model is currently trained primarily on wireframe-style drawings (you can view the training data via `util/visualize_data.ipynb)`, meaning the included model may not perform well for 3D solids. 

Lastly, feel free to send any questions or suggestions to gilbertdyer13@gmail.com