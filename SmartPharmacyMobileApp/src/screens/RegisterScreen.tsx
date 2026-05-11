import React, { useState } from 'react';
import { View, Text, TextInput, TouchableOpacity, StyleSheet, Alert, ActivityIndicator, ScrollView } from 'react-native';
import { AuthService } from '../services/authService';

const RegisterScreen = ({ navigation }: any) => {
  const [name, setName] = useState('');
  const [phone, setPhone] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);

  const handleRegister = async () => {
    if (!name || !phone || !password) {
      Alert.alert('خطأ', 'يرجى ملء الحقول المطلوبة (الاسم، الهاتف، كلمة المرور)');
      return;
    }

    setLoading(true);
    try {
      await AuthService.register({
        fullName: name,
        phoneNumber: phone,
        password,
        email: email || undefined
      });
      Alert.alert('نجاح', 'تم إنشاء الحساب بنجاح، يمكنك الآن تسجيل الدخول', [
        { text: 'حسناً', onPress: () => navigation.navigate('Login') }
      ]);
    } catch (e: any) {
      Alert.alert('خطأ', e.message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <ScrollView contentContainerStyle={styles.container}>
      <View style={styles.headerSection}>
        <Text style={styles.title}>عضوية النخبة</Text>
        <Text style={styles.subtitle}>انضم إلى عالم الرعاية الصحية الفاخرة</Text>
      </View>

      <View style={styles.formCard}>
        <Text style={styles.inputLabel}>الاسم الكامل</Text>
        <TextInput
          style={styles.input}
          placeholder="أدخل اسمك الثلاثي"
          placeholderTextColor="#a4b0be"
          value={name}
          onChangeText={setName}
        />

        <Text style={styles.inputLabel}>رقم الهاتف</Text>
        <TextInput
          style={styles.input}
          placeholder="07xxxxxxxx"
          placeholderTextColor="#a4b0be"
          value={phone}
          onChangeText={setPhone}
          keyboardType="phone-pad"
        />

        <Text style={styles.inputLabel}>البريد الإلكتروني (اختياري)</Text>
        <TextInput
          style={styles.input}
          placeholder="example@mail.com"
          placeholderTextColor="#a4b0be"
          value={email}
          onChangeText={setEmail}
          keyboardType="email-address"
          autoCapitalize="none"
        />

        <Text style={styles.inputLabel}>كلمة المرور</Text>
        <TextInput
          style={styles.input}
          placeholder="••••••••"
          placeholderTextColor="#a4b0be"
          value={password}
          onChangeText={setPassword}
          secureTextEntry
        />

        <TouchableOpacity style={styles.button} onPress={handleRegister} disabled={loading}>
          {loading ? <ActivityIndicator color="#fff" /> : <Text style={styles.buttonText}>تأكيد العضوية</Text>}
        </TouchableOpacity>

        <TouchableOpacity onPress={() => navigation.navigate('Login')} style={styles.loginLink}>
          <Text style={styles.linkPrefix}>لديك عضوية بالفعل؟</Text>
          <Text style={styles.linkText}>سجل دخولك</Text>
        </TouchableOpacity>
      </View>
    </ScrollView>
  );
};

const styles = StyleSheet.create({
  container: {
    flexGrow: 1,
    padding: 20,
    backgroundColor: '#0a3d62', // Royal Navy
    paddingTop: 60,
  },
  headerSection: {
    alignItems: 'center',
    marginBottom: 40,
  },
  title: {
    fontSize: 28,
    fontWeight: 'bold',
    color: '#fff',
    textAlign: 'center',
  },
  subtitle: {
    fontSize: 14,
    color: '#d1d8e0',
    textAlign: 'center',
    marginTop: 5,
  },
  formCard: {
    backgroundColor: '#fff',
    borderRadius: 30,
    padding: 25,
    elevation: 10,
    shadowColor: '#000',
    shadowOpacity: 0.2,
    shadowRadius: 10,
    marginBottom: 30,
  },
  inputLabel: {
    color: '#0a3d62',
    fontSize: 13,
    fontWeight: 'bold',
    marginBottom: 6,
    textAlign: 'right',
  },
  input: {
    backgroundColor: '#f1f2f6',
    padding: 14,
    borderRadius: 12,
    marginBottom: 18,
    textAlign: 'right',
    color: '#2f3542',
    borderWidth: 1,
    borderColor: '#ced6e0',
  },
  button: {
    backgroundColor: '#009432',
    padding: 16,
    borderRadius: 15,
    alignItems: 'center',
    marginTop: 10,
    elevation: 5,
  },
  buttonText: {
    color: '#fff',
    fontSize: 16,
    fontWeight: 'bold',
  },
  loginLink: {
    flexDirection: 'row-reverse',
    justifyContent: 'center',
    marginTop: 20,
  },
  linkPrefix: {
    color: '#747d8c',
    fontSize: 14,
  },
  linkText: {
    color: '#0a3d62',
    fontWeight: 'bold',
    fontSize: 14,
    marginRight: 5,
  },
});

export default RegisterScreen;
