-- Converter 동작 도중에 발생하는 에러와 경고들을 보고하는 기능을 제공합니다

local report = {}

local log_count = {}

local function log(level, message, file, line)
	if log_count[level] == nil then
		log_count[level] = 0
	end
	log_count[level] = log_count[level] + 1

	io.stderr:write("[" .. level .. "]")
	if file then
		io.stderr:write(" " .. file .. ":")
		if line then
			io.stderr:write(line .. ":")
		end
	end
	io.stderr:write(" " .. message .. "\n")
end

function report.warning(message, file, line)
	log("WARNING", message, file, line)
end

function report.error(message, file, line)
	log("ERROR", message, file, line)
end

function report.fatal(message, file, line)
	log("FATAL", message, file, line)
	os.exit(1)
end

function report.get_num_warnings()
	return log_count["WARNING"] or 0
end

function report.get_num_errors()
	return log_count["ERROR"] or 0
end

return report
