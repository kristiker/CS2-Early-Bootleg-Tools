"GameInfo"
{
	game 		"Counter-Strike 2 Tools"
	title 		"Counter-Strike 2 Tools"
	type		singleplayer_only
	nomodels 1
	nohimodel 1

	nodegraph 0
	tonemapping 1 // Show tonemapping ui in tools mode
	GameData	"hlvr.fgd"

	FileSystem
	{
		//
		// The code that loads this file automatically does a few things here:
		//
		// 1. For each "Game" search path, it adds a "GameBin" path, in <dir>\bin
		// 2. For each "Game" search path, it adds another "Game" path in front of it with _<langage> at the end.
		//    For example: c:\hl2\cstrike on a french machine would get a c:\hl2\cstrike_french path added to it.
		// 3. If no "Mod" key, for the first "Game" search path, it adds a search path called "MOD".
		// 4. If no "Write" key, for the first "Game" search path, it adds a search path called "DEFAULT_WRITE_PATH".
		//

		//
		// Search paths are relative to the exe directory\..\
		//
		SearchPaths
		{
			Game				hlvr
			Game				core
			Mod					hlvr
			Write				hlvr
			AddonRoot			hlvr_addons
		}
	}

	MaterialSystem2
	{
		RenderModes
		{
			"game" "Default"
			"game" "VrForward"
			"game" "Depth"
			"game" "Decals"
			"game" "Reflex"

			"dev" "ToolsShadingComplexity"
			"dev" "ToolsVis" // Visualization modes for all shaders (lighting only, normal maps only, etc.)
			"dev" "ToolsWireframe" // This should use the ToolsVis mode above instead of being its own mode
			"tools" "ToolsUtil" // Meant to be used to render tools sceneobjects that are mod-independent, like the origin grid
		}

		ToolsShadingComplexity
		{
			"TargetRenderMode" "VrForward"
		}

		ShaderIDColors
		{
			"generic.vfx" "255 255 255"

			"csgo_simple.vfx" "128 128 128"
			"csgo_complex.vfx" "64 32 128"

			"csgo_static_overlay.vfx" "0 255 255"
			"csgo_projected_decals.vfx" "0 128 128"
			"csgo_glass.vfx" "128 32 128"

			"cables.vfx" "128 64 64"
			"spritecard.vfx" "240 240 0"
		}
	}

	MaterialEditor
	{
		"DefaultShader" "csgo_simple"
	}

	Source1Import
	{
		"importmod"			"ep2"
		"importdir"			"..\hlvr"
		"createStaticOverlays"	"1"	    // if "1" create static overlays from s1 info_overlays, if "0" will treat them as s2 projected decals.
		"createPathParticleRopes" "1"   // convert s1 move_rope/keyframe_rope chained entities to path_particle_rope system in s2.
		"fixup3DSkybox" 		  "1"   // if a func_instance contains a 3d skybox vmf (contains a sky_camera entity), does the s2 fixup accordingly,
										// otherwise the skybox will end up part of the main map and will need to be fixed up by hand
										// (as is the common case in s1 where maps have the skybox already part of the main map).
		"removeHiddenNodes"		  "1"   // 0 => hidden nodes in vmf are imported but marked as visible=false, startenabled=false, 1 => ignore hidden nodes on import
		"loadBSPDetailData"       "1"	// 1 => Load map file .bsp for detail object system (foliage) data.
	}

	Engine2
	{
		"HasModAppSystems" "1"
		"Capable64Bit" "1"
		"URLName" "hlvr"
		"UsesPanorama" "1"
		"UsesBink" "0"
		"PanoramaUIClientFromClient" "1"
		"RenderingPipeline"
		{
			"SkipPostProcessing" "0"
			"PostProcessingInMainPipeline" "1"
			"TonemappingVRForward" "1"
			"Tonemapping_UseLogLuminance" "1"
			"ToolsVisModes" "1"
			"OpaqueFade" "1"
			"AmbientOcclusionProxies" "1"
			"RetinaBurnFx" "1"
			"HighPrecisionLighting" "1"
		}
		// Default MSAA sample count when run in non-VR mode
		"MSAADefaultNonVR"	"4"
		"SuppressNonConsoleVGuiInVR" "1"
	}

	SceneSystem
	{
		"SunLightMaxCascadeSize" "0"
		"SunLightMaxShadowBufferSize" "0"
		"SunLightMaxCascadeSizeTools" "2"
		"SunLightMaxShadowBufferSizeTools" "2048"
		"SunLightShadowRenderMode" "Depth"
		"VrLightBinner" "1"
		"VrLightBinnerMaxLights" "128" // must be the same as MAX_LIGHTS in vr_lighting.fxc
		"VrLightBinnerSingleLightFastPath" "1"
		"VrDefaultShadowTextureWidth" "4096"
		"VrDefaultShadowTextureHeight" "4096"
		"VrShadowDepthMasks" "1"
		"VrLightBinnerShadowViewsNeedPerViewLightingConstants" "1"
		"PointLightShadowsEnabled" "1"
		"Tonemapping"	"1"
		"VolumetricFog" "1"
		"NonTexturedGradientFog" "1"
		"CubemapFog" "1"
		"BloomEnabled" "1"
		"HDRFrameBuffer" "1"
		"DisableShadowFullSort" "1"
		"PerObjectLightingSetup" "1"
		"CharacterDecals" "1"
		"EnvironmentalInteraction" "1"
		"VrTeleportPathRendering" "1"
		"MaxAutoPartitions" "8"
		"TransformFormat" "TINTRGBA8_ENVMAPIDX"
	}

	ToolsEnvironment
	{
		"Engine"	"Source 2"
		"ToolsDir"	"../sdktools"	// NOTE: Default Tools path. This is relative to the mod path.
	}

	Hammer
	{
		"fgd"							"hlvr.fgd"	// NOTE: This is relative to the 'game' path.
		"GameFeatureSet"				"HalfLife"
		"DefaultTextureScale"			"0.250000"
		"DefaultSolidEntity"			"func_buyzone"
		"DefaultPointEntity"			"info_player_terrorist"
		"DefaultPathEntity"				"path_particle_rope"
		"NavMarkupEntity"				"func_nav_markup"
		"OverlayBoxSize"				"8"
		"UsesBakedLighting"				"1"
		"TileMeshesEnabled"				"1"
		"DefaultMinDrawVolumeSize"		"128"
		"RenderMode"					"ToolsVis"
		"DefaultMinTrianglesPerCluster"	"2048"
		"CreateRenderClusters"			"1"
		"ShadowAtlasWidth"				"6144"
		"ShadowAtlasHeight"				"6144"
		"TimeSlicedShadowMapRendering"	"1"
		"SteamAudioEnabled"				"1"
		"Required3dSkyboxEntities"		"sky_camera"
		"ModelStateAutoConversionEntities"
		{
			"0"	"prop_ragdoll"
		}
		"AddonMapCommand"				"addon_tools_map"
		"AddonMapCommandIsAddonImplied"	"1"
	}

	ModelDoc
	{
		"models_gamedata"			"models_gamedata.fgd"
		"export_modeldoc"			"1"
		"features"					"animgraph;modelconfig"
	}

	PostProcessingEditor
	{
		"supports_vignette"			"0"
	}

	RenderPipelineAliases
	{
		"Tools"			"VR"
		"EnvMapBake"	"VR"
	}

	SoundTool
	{
		"DefaultSoundEventType" "hlvr_default_3d"
	}

	BugReporter
	{
		"AutoBugProduct" "Half-Life VR"
	}

	ResourceCompiler
	{
		// Overrides of the default builders as specified in code, this controls which map builder steps
		// will be run when resource compiler is run for a map without specifiying any specific map builder
		// steps. Additionally this controls which builders are displayed in the hammer build dialog.
		DefaultMapBuilders
		{
			"bakedlighting"	"1"	// Enable lightmapping during compile time
			"envmap"		"0"	// Using env_cubemap entities
			"nav"			"1"	// Generate nav mesh data
			"sareverb"      "1" // Bake Steam Audio reverb
			"sapaths"		"1" // Bake Steam Audio pathing info
		}

		TextureCompiler
		{
			MinRoughness			"0.01"	// Minimum roughness value for PBR
			MaxRoughnessAnisotropy	"8.0"	// Maximum roughness anisotropy for PBR
			CompressMipsOnDisk      "1"
			CompressMinRatio        "95"
		}

		MeshCompiler
		{
			PerDrawCullingData      "1"
			EncodeVertexBuffer      "1"
			EncodeIndexBuffer       "1"
		}

		WorldRendererBuilder
		{
			FixTJunctionEdgeCracks  		"1"
			VisibilityGuidedMeshClustering		"1"
			MinimumTrianglesPerClusteredMesh	"2048"
			MinimumVerticesPerClusteredMesh		"2048"
			MinimumVolumePerClusteredMesh		"1800"		// ~12x12x12 cube
			MaxPrecomputedVisClusterMembership	"48"
			UseAggregateInstances			"1"
			AggregateInstancingMeshlets			"1"
			UseModelDoc							"1"
			UseStaticEnvMapForObjectsWithLightingOrigin	"1"
		}

		PhysicsBuilder
		{
			DefaultHammerMeshSimplification	"0.1"
		}

		BakedLighting
		{
			Version 2
			DisableCullingForShadows 1
			MinSpecLightmapSize 4096
			LPVAtlas 1
			LPVOctree 0
			LightmapChannels
			{
				irradiance 1
				direct_light_shadows 1

				directional_irradiance
				{
					MaxResolution 4096
					CompressedFormat DXT1
				}

				debug_chart_color
				{
					MaxResolution 4096
					CompressedFormat DXT1
				}
			}
		}

		VisBuilder
		{
			MaxVisClusters "4096"
			PreMergeOpenSpaceDistanceThreshold "128.0"
			PreMergeOpenSpaceMaxDimension "2048.0"
			PreMergeOpenSpaceMaxRatio "8.0"
			PreMergeSmallRegionsSizeThreshold "20.0"
		}

		SteamAudio
		{
			Probes
			{
				GridSpacing			"3.0"
				HeightAboveFloor	"1.5"
			}
			Reverb
			{
				NumRays				"32768"
				NumBounces			"64"
				IRDuration			"1.0"
				AmbisonicsOrder		"1"
			}
			Pathing
			{
				NumVisSamples		"1"
				ProbeVisRadius		"3.0"
				ProbeVisThreshold	"0.01"
				ProbePathRange		"1000.0"
			}
		}

		VisBuilder
		{
			MaxVisClusters "2048"
		}
	}

	WorldRenderer
	{
		"IrradianceVolumes"		"0"
		"EnvironmentMaps"		"1"
		"EnvironmentMapFaceSize" "256"
		"EnvironmentMapRenderSize" "1024"
		"EnvironmentMapFormat" "BC6H"
		"EnvironmentMapColorSpace" "linear"
		"EnvironmentMapMipProcessor" "GGXCubeMapBlur"
		// Build cubemaps into a cube array instead of individual cubemaps.
		"EnvironmentMapUseCubeArray" "1"
	}

	NavSystem
	{
		"NavTileSize" "128.0"
		"NavCellSize" "1.5"
		"NavCellHeight" "2.0"

		"NavAgentNumHulls" "3"

		"NavAgentRadius_0" "12.0"
		"NavAgentHeight_0" "24.0"
		"NavAgentMaxClimb_0" "15.5"
		"NavAgentMaxSlope_0" "50"
		"NavAgentMaxJumpDownDist_0" "0.0"
		"NavAgentMaxJumpUpDist_0" "0.0"
		"NavAgentMaxJumpHorizDistBase_0" "64.0"

		"NavAgentRadius_1" "16.0"
		"NavAgentHeight_1" "79.0"
		"NavAgentMaxClimb_1" "15.5"
		"NavAgentMaxSlope_1" "50"
		"NavAgentMaxJumpDownDist_1" "0.0"
		"NavAgentMaxJumpUpDist_1" "0.0"
		"NavAgentMaxJumpHorizDistBase_1" "64.0"

		"NavAgentRadius_2" "23.0"
		"NavAgentHeight_2" "69.0"
		"NavAgentMaxClimb_2" "15.5"
		"NavAgentMaxSlope_2" "50"
		"NavAgentMaxJumpDownDist_2" "240.0"
		"NavAgentMaxJumpUpDist_2" "240.0"
		"NavAgentMaxJumpHorizDistBase_2" "64.0"

		"NavRegionMinSize" "8"
		"NavRegionMergeSize" "20"
		"NavEdgeMaxLen" "1200"
		"NavEdgeMaxError" "51.0"
		"NavVertsPerPoly" "4"
		"NavDetailSampleDistance" "120.0"
		"NavDetailSampleMaxError" "2.0"
		"NavSmallAreaOnEdgeRemovalSize" "-1.0"
	}

	RenderingBudget
	{
		"ArtistTrianglesRendered"	"1000000"
	}

	PhysicsBudget
	{
		"HullBudgetMin" "16"
		"HullBudgetMax" "64"
		"MeshBudgetMin" "64"
		"MeshBudgetMax" "512"
	}

	RenderSystem
	{
		"AllowSampleableDepthInVr" "1"
		"AllowPartialMipChainImmediateTexLoads" "1"
		"MaxPreloadTextureResolution" "256"
		"MinStreamingPoolSizeMB" "1536"
		"SetInitialStreamingPoolSizeFromBudget" "1"
		"VulkanUseStreamingTextureManager" "1"
		"VulkanMutableSwapchain" "1"
		"VulkanSteamShaderCache" "1"
		"VulkanSteamAppShaderCache" "1"
		"VulkanSteamDownloadedShaderCache" "0"
		"VulkanAdditionalShaderCache" "vulkan_shader_cache.foz"
		"VulkanStagingPMBSizeLimitMB" "128"
	}

	Particles
	{
		FeatureID						324234
		PET_AllowScreenSpaceOverlay		1
		PET_SupportFadingOpaqueModels	1
		Budget_SimulationUS				100
		Budget_RenderUS					100
		Budget_InterectionTests			20
		Budget_RealTraces				5
		Budget_EstimatedTraceCostUS		15
		Budget_OverallocationThreshold	2.5
	}

	AnimationSystem
	{
		NumDecodeCaches "16"
		DecodeCacheMemoryKB "512"
	}

	SoundSystem
	{
		"SteamAudioEnabled"            "1"
		Budget_StackSimulationUS		25
		Budget_FirstStackSimulationUS	50
	}

	GameInstructor
	{
		"SaveToSavegames"				"1"
	}

	Vr
	{
		"PreAllocateScratchRenderTargets"	"0"
		"FilterFidelityLevelsByGPUMemoryRatio"	"1"
		"MultiviewInstancing"				"1"
		"TickRateLockedToDisplayRate"		"1"
		"VrFidelityConfigName"				"hlvr"
	}
}


