# Umbraco.VideoProcessing.Demo

Umbraco.VideoProcessing.Demo to demo a video processing flow including:

- User upload video in Umbraco
- Video is processed
    - Extract metadata
    - Slice video
    - Create playlist
    - Convert video part into 360, 480, 720, 1080
    - Save and monitor process status
- Stream video to user.

## Main projects:

### UmbracoSite

Project to upload and view video.

### Jobs.InitiateVideoProcessing

Project to initiate video processing, make sure video is not processed twice.

### Jobs.SliceVideo

Projct to slice video into multiple parts

### Jobs.SelectResolution

Project to read resolution configuration and emit events to convert video parts to different resolutions. 

### Jobs.ConvertVideo

Project to convert video parts to different resolutions.

### Jobs.MonitorProcessing

Project to capture events in the processing to update the processing status.

### CronJobs.SupportTask

Project to delete old converted files, requeue converted videos with errors, redo error processing task...

## Usage

- Use Dev Containers: Clone Repository in Container Volume
- RUN `make run-all`

