using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BedrockService.Shared.Classes
{
    [Serializable]
    public class RSAContainer
    {
        public byte[] D;
        public byte[] DP;
        public byte[] DQ;
        public byte[] Exponent;
        public byte[] InverseQ;
        public byte[] Modulus;
        public byte[] P;
        public byte[] Q;

        public RSAContainer(RSAParameters input)
        {
            D = input.D;
            P = input.DP;
            DP = input.DP;
            Q = input.DQ;
            DQ = input.DQ;
            InverseQ = input.InverseQ;
            Modulus = input.Modulus;
            Exponent = input.Exponent;
        }

        public RSAParameters GetPrivateKey()
        {
            return new RSAParameters()
            {
                D = this.D,
                P = this.P,
                DP = this.DP,
                Q = this.Q,
                DQ = this.DQ,
                InverseQ = this.InverseQ,
                Exponent = this.Exponent,
                Modulus = this.Modulus
            };
        }

        public RSAParameters GetPublicKey()
        {
            return new RSAParameters()
            {
                Modulus = this.Modulus,
                Exponent = this.Exponent
            };
        }

        public void SetPrivateKey(RSAParameters privateKey)
        {
            this.D = privateKey.D;
            this.DP = privateKey.DP;
            this.P = privateKey.P;
            this.DQ = privateKey.DQ;
            this.Q = privateKey.Q;
            this.InverseQ = privateKey.InverseQ;
            this.Modulus = privateKey.Modulus;
            this.Exponent = privateKey.Exponent;
        }

        public void SetPublicKey(RSAParameters publicKey)
        {
            this.Exponent = publicKey.Exponent;
            this.Modulus = publicKey.Modulus;
        }
    }

    [Serializable]
    public class CommsKeyContainer
    {
        public RSAContainer LocalPrivateKey = new RSAContainer(new RSAParameters());
        public RSAContainer RemotePublicKey = new RSAContainer(new RSAParameters());
        public byte[] AesKey;
        public byte[] AesIV;

        public CommsKeyContainer() { }

        public CommsKeyContainer (RSAParameters priv, RSAParameters pub, byte[] key, byte[] IV)
        {
            LocalPrivateKey = new RSAContainer(priv);
            RemotePublicKey = new RSAContainer(pub);
            AesKey = key;
            AesIV = IV;
        }
    }
}
