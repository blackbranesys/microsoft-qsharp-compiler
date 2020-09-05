using System;
using System.Collections.Generic;
using System.Text;

using Bond;
using Bond.IO.Unsafe;
using Bond.Protocols;

namespace Microsoft.Quantum.QsCompiler
{
    public static class PerformanceExperiments
    {

        public static (Serializer<FastBinaryWriter<OutputBuffer>>, FastBinaryWriter<OutputBuffer>, OutputBuffer)CreateFastBinaryBufferSerializationTuple()
        {
            var outputBuffer = new OutputBuffer();
            var serializer = new Serializer<FastBinaryWriter<OutputBuffer>>(typeof(BondSchemas.QsCompilation));
            var fastBinaryWriter = new FastBinaryWriter<OutputBuffer>(outputBuffer);
            return (serializer, fastBinaryWriter, outputBuffer);
        }

        public static (Deserializer<FastBinaryReader<InputBuffer>>, FastBinaryReader<InputBuffer>) CreateFastBinaryBufferDeserializationTuple(
            OutputBuffer dataBuffer)
        {
            var inputBuffer = new InputBuffer(dataBuffer.Data);
            var deserializer = new Deserializer<FastBinaryReader<InputBuffer>>(typeof(BondSchemas.QsCompilation));
            var reader = new FastBinaryReader<InputBuffer>(inputBuffer);
            return (deserializer, reader);
        }
    }
}
