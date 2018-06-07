print("hello world")

function DoFile( filePath )
	require(filePath)
end

function DoModule( filePath )
	return require(filePath)()
end