echo ""

PROTOC=../../../../External/protoc/3.2.0/bin/protoc.exe
SRC_DIR=.
PROTO_DST_DIR=../Source/
PROTO_FILES=("NavMesh.proto")

echo Compiler: $PROTOC
$PROTOC --version
echo ""

echo Generating Protocol
mkdir -p $PROTO_DST_DIR
for i in "${PROTO_FILES[@]}"
do
$PROTOC --proto_path=Protocol=. --cpp_out=$PROTO_DST_DIR $i
done

echo ""
