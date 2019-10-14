#! /bin/bash
PLATFORM=$1
TYPE=$2
COVERAGE=$3
WHERE="Category!=ManualTest"
TEST_PATTERN="*Test.dll"
ASSEMBLIES=""
TEST_LOG_FILE="TestLog.txt"

echo "test dir: $TEST_DIR"
if [ -z "$TEST_DIR" ]; then
    TEST_DIR="."
fi

if [ -d "$TEST_DIR/_tests" ]; then
  TEST_DIR="$TEST_DIR/_tests"
fi

COVERAGE_RESULT_DIRECTORY="$TEST_DIR/CoverageResults/"

rm -f "$TEST_LOG_FILE"

# Uncomment to log test output to a file instead of the console
export RADARR_TESTS_LOG_OUTPUT="File"

NUNIT="dotnet vstest"
NUNIT_COMMAND="$NUNIT"
NUNIT_PARAMS="--Platform:x64 --logger:nunit;LogFilePath=TestResult.xml"

if [ "$PLATFORM" = "Mac" ]; then

  export DYLD_FALLBACK_LIBRARY_PATH="$TEST_DIR:$MONOPREFIX/lib:/usr/local/lib:/lib:/usr/lib"
  echo $DYLD_FALLBACK_LIBRARY_PATH
  mono --version

  # To debug which libraries are being loaded:
  # export DYLD_PRINT_LIBRARIES=YES
fi

if [ "$PLATFORM" = "Windows" ]; then
  mkdir -p "$ProgramData/Radarr"
  WHERE="$WHERE&Category!=LINUX"
elif [ "$PLATFORM" = "Linux" ] || [ "$PLATFORM" = "Mac" ] ; then
  mkdir -p ~/.config/Radarr
  WHERE="$WHERE&Category!=WINDOWS"
else
  echo "Platform must be provided as first arguement: Windows, Linux or Mac"
  exit 1
fi

if [ "$TYPE" = "Unit" ]; then
  WHERE="$WHERE&Category!=IntegrationTest&Category!=AutomationTest"
elif [ "$TYPE" = "Integration" ] || [ "$TYPE" = "int" ] ; then
  WHERE="$WHERE&Category=IntegrationTest"
elif [ "$TYPE" = "Automation" ] ; then
  WHERE="$WHERE&Category=AutomationTest"
else
  echo "Type must be provided as second argument: Unit, Integration or Automation"
  exit 2
fi

for i in `find $TEST_DIR -name "$TEST_PATTERN"`;
  do ASSEMBLIES="$ASSEMBLIES $i"
done

if [ "$COVERAGE" = "Coverage" ]; then
  if [ "$PLATFORM" = "Windows" ] || [ "$PLATFORM" = "Linux" ]; then
    dotnet tool install coverlet.console --tool-path="$TEST_DIR/coverlet/"
    mkdir $COVERAGE_RESULT_DIRECTORY
    OPEN_COVER="$TEST_DIR/coverlet/coverlet"
    $OPEN_COVER "$TEST_DIR/" --verbosity "detailed" --format "cobertura" --format "opencover" --output "$COVERAGE_RESULT_DIRECTORY" --exclude "[Radarr.*.Test]*" --exclude "[Radarr.Test.*]*" --exclude "[Radarr.Api*]*" --exclude "[Marr.Data]*" --exclude "[MonoTorrent]*" --exclude "[CurlSharp]*" --target "$NUNIT" --targetargs "$NUNIT_PARAMS --where=\"$WHERE\" $ASSEMBLIES";
    EXIT_CODE=$?
  else
    echo "Coverage only supported on Windows and Linux"
    exit 3
  fi
elif [ "$COVERAGE" = "Test" ] ; then
  echo "$NUNIT_COMMAND $ASSEMBLIES --TestCaseFilter:$WHERE $NUNIT_PARAMS"
  $NUNIT_COMMAND $ASSEMBLIES --TestCaseFilter:"$WHERE" $NUNIT_PARAMS
  EXIT_CODE=$?
else
  echo "Run Type must be provided as third argument: Coverage or Test"
  exit 3
fi

if [ "$EXIT_CODE" -ge 0 ]; then
  echo "Failed tests: $EXIT_CODE"
  exit 0
else
  exit $EXIT_CODE
fi
