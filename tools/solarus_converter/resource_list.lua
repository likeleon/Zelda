-- project_db.dat로부터 퀘스트 리소스 리스트를 구성해서 반환합니다

local importer = {}

local report = require("report")

local resource_type_names = {
  "map",
  "tileset",
  "music",
  "sprite",
  "sound",
  "item",
  "enemy",
  "language",
  "entity",
  "font",
}

function importer.check(quest_path)
	print("Reading the resource list project_db.dat")

	local resources = {}

	-- 각 리소스 타입마다 테이블을 생성합니다
	local env = {}

	for _, resource_type_name in ipairs(resource_type_names) do
		local resource = {}
		resources[resource_type_name] = resource
		env[resource_type_name] = function(resource_element)
			resource[#resource + 1] = resource_element
		end
	end

	local file = quest_path .. "project_db.dat"
	local chunk, error = loadfile(file)
	if chunk == nil then
		report.fatal("Error in resource list file: " .. error)
	else
		setfenv(chunk, env)
		local success, error = pcall(chunk)

		if not success then
			report.fatal("Error in resource list file: " .. error)
		end
	end

	return resources
end

return importer
