using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvilsoftCommons.Exceptions.UUIDGenerator {

    /// <summary>
    /// Represents an immutable Java universally unique identifier (UUID).
    /// A UUID represents a 128-bit value.
    /// </summary>
    public struct UuidGenerator : IEquatable<UuidGenerator> {
        private readonly long leastSignificantBits;
        private readonly long mostSignificantBits;

        /// <summary>
        /// Constructs a new UUID using the specified data.
        /// </summary>
        /// <param name="mostSignificantBits">The most significant 64 bits of the UUID.</param>
        /// <param name="leastSignificantBits">The least significant 64 bits of the UUID</param>
        public UuidGenerator(long mostSignificantBits, long leastSignificantBits) {
            this.mostSignificantBits = mostSignificantBits;
            this.leastSignificantBits = leastSignificantBits;
        }

        /// <summary>
        /// The least significant 64 bits of this UUID's 128 bit value.
        /// </summary>
        public long LeastSignificantBits {
            get { return leastSignificantBits; }
        }

        /// <summary>
        /// The most significant 64 bits of this UUID's 128 bit value.
        /// </summary>
        public long MostSignificantBits {
            get { return mostSignificantBits; }
        }

        /// <summary>
        /// Returns a value that indicates whether this instance is equal to a specified
        /// object.
        /// </summary>
        /// <param name="o">The object to compare with this instance.</param>
        /// <returns>true if o is a <paramref name="uuid"/> that has the same value as this instance; otherwise, false.</returns>
        public override bool Equals(object obj) {
            if (obj == null || !(obj is UuidGenerator)) {
                return false;
            }

            UuidGenerator uuidGenerator = (UuidGenerator)obj;

            return Equals(uuidGenerator);
        }

        /// <summary>
        /// Returns a value that indicates whether this instance and a specified <see cref="UuidGenerator"/>
        /// object represent the same value.
        /// </summary>
        /// <param name="uuidGenerator">An object to compare to this instance.</param>
        /// <returns>true if <paramref name="uuidGenerator"/> is equal to this instance; otherwise, false.</returns>
        public bool Equals(UuidGenerator uuidGenerator) {
            return this.mostSignificantBits == uuidGenerator.mostSignificantBits && this.leastSignificantBits == uuidGenerator.leastSignificantBits;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override int GetHashCode() {
            return ((Guid)this).GetHashCode();
        }

        /// <summary>
        /// <para></para>
        /// Returns a String object representing this UUID.
        /// </para>
        /// <para>
        /// The UUID string representation is as described by this BNF:
        /// </para>
        /// <code>
        ///   UUID                   =  "-"  "-"
        ///                             "-"
        ///                             "-"
        ///                            
        ///   time_low               = 4*
        ///   time_mid               = 2*
        ///   time_high_and_version  = 2*
        ///   variant_and_sequence   = 2*
        ///   node                   = 6*
        ///   hexOctet               = 
        ///   hexDigit               =
        ///         "0" | "1" | "2" | "3" | "4" | "5" | "6" | "7" | "8" | "9"
        ///         | "a" | "b" | "c" | "d" | "e" | "f"
        ///         | "A" | "B" | "C" | "D" | "E" | "F"
        /// </code>
        /// </summary>
        /// <returns>A string representation of this UUID.</returns>
        public override string ToString() {
            return ((Guid)this).ToString();
        }

        /// <summary>Indicates whether the values of two specified <see cref="T:UuidGenerator" /> objects are equal.</summary>
        /// <returns>true if <paramref name="a" /> and <paramref name="b" /> are equal; otherwise, false.</returns>
        /// <param name="a">The first object to compare. </param>
        /// <param name="b">The second object to compare. </param>
        public static bool operator ==(UuidGenerator a, UuidGenerator b) {
            return a.Equals(b);
        }

        /// <summary>Indicates whether the values of two specified <see cref="T:UuidGenerator" /> objects are not equal.</summary>
        /// <returns>true if <paramref name="a" /> and <paramref name="b" /> are not equal; otherwise, false.</returns>
        /// <param name="a">The first object to compare. </param>
        /// <param name="b">The second object to compare. </param>
        public static bool operator !=(UuidGenerator a, UuidGenerator b) {
            return !a.Equals(b);
        }

        /// <summary>Converts an <see cref="T:UuidGenerator"/> to a <see cref="T:System.Guid" />.</summary>
        /// <param name="value">The value to convert. </param>
        /// <returns>A <see cref="T:System.Guid"/> that represents the converted <see cref="T:UuidGenerator" />.</returns>
        public static explicit operator Guid(UuidGenerator uuidGenerator) {
            if (uuidGenerator == default(UuidGenerator)) {
                return default(Guid);
            }

            byte[] uuidMostSignificantBytes = BitConverter.GetBytes(uuidGenerator.mostSignificantBits);
            byte[] uuidLeastSignificantBytes = BitConverter.GetBytes(uuidGenerator.leastSignificantBits);
            byte[] guidBytes = new byte[16] {
                uuidMostSignificantBytes[4],
                uuidMostSignificantBytes[5],
                uuidMostSignificantBytes[6],
                uuidMostSignificantBytes[7],
                uuidMostSignificantBytes[2],
                uuidMostSignificantBytes[3],
                uuidMostSignificantBytes[0],
                uuidMostSignificantBytes[1],
                uuidLeastSignificantBytes[7],
                uuidLeastSignificantBytes[6],
                uuidLeastSignificantBytes[5],
                uuidLeastSignificantBytes[4],
                uuidLeastSignificantBytes[3],
                uuidLeastSignificantBytes[2],
                uuidLeastSignificantBytes[1],
                uuidLeastSignificantBytes[0]
            };

            return new Guid(guidBytes);
        }

        /// <summary>Converts a <see cref="T:System.Guid" /> to an <see cref="T:UuidGenerator"/>.</summary>
        /// <param name="value">The value to convert. </param>
        /// <returns>An <see cref="T:UuidGenerator"/> that represents the converted <see cref="T:System.Guid" />.</returns>
        public static implicit operator UuidGenerator(Guid value) {
            if (value == default(Guid)) {
                return default(UuidGenerator);
            }

            byte[] guidBytes = value.ToByteArray();
            byte[] uuidBytes = new byte[16] {
                guidBytes[6],
                guidBytes[7],
                guidBytes[4],
                guidBytes[5],
                guidBytes[0],
                guidBytes[1],
                guidBytes[2],
                guidBytes[3],
                guidBytes[15],
                guidBytes[14],
                guidBytes[13],
                guidBytes[12],
                guidBytes[11],
                guidBytes[10],
                guidBytes[9],
                guidBytes[8]
            };

            return new UuidGenerator(BitConverter.ToInt64(uuidBytes, 0), BitConverter.ToInt64(uuidBytes, 8));
        }

        /// <summary>
        /// Creates a UUID from the string standard representation as described in the <see cref="ToString()"/> method.
        /// </summary>
        /// <param name="input">A string that specifies a UUID.</param>
        /// <returns>A UUID with the specified value.</returns>
        /// <exception cref="ArgumentNullException">input is null.</exception>
        /// <exception cref="FormatException">input is not in a recognized format.</exception>
        public static UuidGenerator FromString(string input) {
            return (UuidGenerator)Guid.Parse(input);
        }
    }
}
