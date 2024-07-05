.PHONY: run-all
.PHONY: run-umbraco
.PHONY: run-InitiateVideoProcessing
.PHONY: run-SliceVideo
.PHONY: run-SelectResolution
.PHONY: run-ConvertVideo
.PHONY: run-MonitorProcessing

run-umbraco:
	dotnet run --project src/UmbracoSite/

run-InitiateVideoProcessing:
	dotnet run --project src/Jobs.InitiateVideoProcessing/

run-SliceVideo:
	dotnet run --project src/Jobs.SliceVideo/

run-SelectResolution:
	dotnet run --project src/Jobs.SelectResolution/

run-ConvertVideo:
	dotnet run --project src/Jobs.ConvertVideo/

run-MonitorProcessing:
	dotnet run --project src/Jobs.MonitorProcessing/

run-all:
	make -j 6 run-umbraco run-InitiateVideoProcessing run-SliceVideo run-SelectResolution run-ConvertVideo run-MonitorProcessing